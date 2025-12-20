using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;
using static ShortCutKeysLib.KeySequenceMngr;

namespace ShortCutKeysLib
{
    public class KeyComb
    {
        public Key Key { get; set; }
        //public List<ModifierKeys>? Mods { get; set; }
        public ModifierKeys Mods { get; set; } = ModifierKeys.None;

        [JsonIgnore]
        public DateTime? LastPressedTime { get; set; }
        public bool PressedRecently()
        {
            if (LastPressedTime == null) { return false; }
            return (DateTime.Now - LastPressedTime.Value).TotalSeconds <= KeySequenceMngr.SEQUENCE_TIMEOUT_SECONDS;
        }
        public void Press()
        {
            LastPressedTime = DateTime.Now;
        }

        //internal bool Check(Key key, List<ModifierKeys> mods)
        internal bool Check(Key key, ModifierKeys mods)
        {
            if (Key != key) { return false; }

            return Mods == mods;
        }

        internal bool IsEqual(KeyComb kc)
        {
            return kc.Key == Key && kc.Mods == Mods;
        }
    }

    public enum CompareResult
    {
        NotEqual,
        StartsWith,
        Equivalent,
        Overlaps,
    }

    public class KeyCombSequence
    {

        public override string ToString() => $"{Name}";

        [JsonIgnore]
        public object? Sender { get; set; }

        public KeyCombSequence()
        {
                
        }

        public KeyCombSequence(string name, string desc, Action<KeyEventArgs, object> action, string sender, params (Key, ModifierKeys)[] keysAndMods)
        {
            Name = name;
            Desc = desc;
            KeyCombs = new List<KeyComb>();
            foreach (var (key, mods) in keysAndMods)
            {
                KeyCombs.Add(new KeyComb() { Key = key, Mods = mods });
            }
            Action = action;
            Sender = sender;
            LstPressedTimes = Enumerable.Repeat<DateTime?>(null, KeyCombs.Count).ToList();
        }

        public KeyCombSequence(string name, string desc, List<KeyComb> keyCombs, Action<KeyEventArgs, object> action, object? sender = null)
        {
            Name = name;
            Desc = desc;
            KeyCombs = keyCombs;
            Action = action;
            Sender = sender;
            //LstPressedTimes = new List<DateTime>(new DateTime[keyCombs.Count]);
            LstPressedTimes = Enumerable.Repeat<DateTime?>(null, keyCombs.Count).ToList();
        }

        public string Name { get; }
        public string Desc { get; }
        public List<KeyComb> KeyCombs { get; set; }

        [JsonIgnore]
        public List<DateTime?> LstPressedTimes { get; set; } = new List<DateTime?>();

        [JsonIgnore]
        public Action<KeyEventArgs, object> Action { get; set; }

        public bool CheckKeyComb(KeyEventArgs e, object? sender = null)
        {
            var mods = Keyboard.Modifiers;

            return CheckKeyComb(e.Key, mods);
        }

        public bool CheckKeyComb(Key key, ModifierKeys mods)
        {
            for (int i = 0; i < KeyCombs.Count; i++)
            {
                var keyC = KeyCombs.ElementAt(i);
                var pressed = keyC.PressedRecently();
                if (pressed) { continue; }

                bool match = keyC.Check(key, mods);
                if (match)
                {
                    keyC.Press();

                    if (i == KeyCombs.Count - 1)
                    {
                        // Last key in sequence matched
                        return true;
                    }
                    else
                    {
                        // Not last key, wait for the next
                        break;
                    }
                }
                // If we reach here, either the key sequence didn't match or it wasn't pressed recently
                return false;
            }
            return false;
        }

        public string KeyCombString()
        {
            //KeyCombs.Select(k => $"{k.Key} + {ModsToStr(k.Mods)} ");

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < KeyCombs.Count; i++)
            {
                var kc = KeyCombs[i];
                if (kc.Mods != ModifierKeys.None)
                {
                    sb.Append(kc.Mods.ToString().Replace(", ", "+"));
                    sb.Append("+");
                }
                sb.Append(kc.Key.ToString());
                if (i < KeyCombs.Count - 1)
                {
                    sb.Append(" , ");
                }
            }
            return sb.ToString();
        }


        internal CompareResult Compare(List<KeyComb> enteredKeyComb)
        {
            CompareResult compareResult = CompareResult.NotEqual;

            //bool startingSubset = false;
            //bool identical = false;
            bool comparison = this.CompareKeyCombs(enteredKeyComb);

            if (comparison)
            {
                if (enteredKeyComb.Count < KeyCombs.Count)
                {
                    compareResult = CompareResult.StartsWith;
                }
                else if (enteredKeyComb.Count == KeyCombs.Count)
                {
                    compareResult = CompareResult.Equivalent;
                }
                else if (enteredKeyComb.Count > KeyCombs.Count)
                {
                    compareResult = CompareResult.Overlaps;
                }
            }
            return compareResult;
        }

        private bool CompareKeyCombs(List<KeyComb> enteredKeyComb)
        {
            for (int i = 0; i < KeyCombs.Count; i++)
            {
                if (enteredKeyComb.Count > i)
                {
                    if (!KeyCombs[i].IsEqual(enteredKeyComb[i]))
                    {
                        return false;
                    }
                }
                else
                {
                    return true; // was equal until run out of keycombs
                }
            }
            return true;
        }
    }

    public partial class KeySequenceMngr
    {
        private KeySeqStateManager StateManager { get; set; }

        public List<KeyCombSequence> KeyCombSequences { get; set; }

        

        public const double SEQUENCE_TIMEOUT_SECONDS = 1.0f;

        public const string DEFAULT_STATE_FILE_NAME = "KeySeqState.json";

        public KeySequenceMngr(string confFileName) {
            KeyCombSequences = new List<KeyCombSequence>();
            StateManager = new KeySeqStateManager(confFileName != null? confFileName: DEFAULT_STATE_FILE_NAME);
            StateManager.LoadState(this);
        }

        public KeySequenceMngr(List<KeyCombSequence> keyCombSeqs, string confFileName)
        {
            KeyCombSequences = keyCombSeqs;
            StateManager = new KeySeqStateManager(confFileName != null ? confFileName : DEFAULT_STATE_FILE_NAME);
            StateManager.LoadState(this);
        }

        public KeySequenceMngr(
            List<(List<Key> keys, Action<KeyEventArgs, object> act, object? sender, string name, string desc)> controlKeyShortCuts, 
            List<KeyCombSequence> keyCombSeqs,
            string confFileName) 
            : base()
        {
            KeyCombSequences = keyCombSeqs != null ? keyCombSeqs : new List<KeyCombSequence>();
            AddControlSequences(controlKeyShortCuts);
            StateManager = new KeySeqStateManager(confFileName != null ? confFileName : DEFAULT_STATE_FILE_NAME);
            StateManager.LoadState(this);
        }


        ~KeySequenceMngr()
        {
            StateManager.SaveState(this);
        }

        public void AddKeyCombSequence(KeyCombSequence keyCombSeq) 
        {
            if (!KeyCombSequences.Any(k => k.Name == keyCombSeq.Name)) { KeyCombSequences.Add(keyCombSeq); }
            else { ShowNotAdded(new List<string>() { keyCombSeq.Name }); }
        }

        public void AddKeyCombSequenceRange(List<KeyCombSequence> keyCombSeqs)
        {
            List<string> existingNames = KeyCombSequences.Select(k => k.Name).ToList();
            keyCombSeqs = keyCombSeqs.Where(kcs => !existingNames.Contains(kcs.Name)).ToList();
            KeyCombSequences.AddRange(keyCombSeqs);

            var allreadyExist = keyCombSeqs.Where(kcs => existingNames.Contains(kcs.Name)).Select(k => k.Name).ToList();
            ShowNotAdded(allreadyExist);
        }

        private static void ShowNotAdded(List<string> allreadyExist)
        {
            if (allreadyExist.Count > 0)
            {
                string msg = "The following Key Combination Sequences were not added because they allready exist:\n" +
                    string.Join("\n", allreadyExist);
                System.Windows.MessageBox.Show(msg, "Key Combination Sequences allready exist", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        private static void ShowOverridden(List<string> allreadyExist)
        {
            if (allreadyExist.Count > 0)
            {
                string msg = "The following Key Combination Sequences were overridden because they allready exist:\n" +
                    string.Join("\n", allreadyExist);
                System.Windows.MessageBox.Show(msg, "Key Combinations overridden", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        public void CheckSequences(KeyEventArgs e, object sender = null)
        {
            DateTime now = DateTime.Now;

            foreach (var keySequence in KeyCombSequences)
            {
                if (keySequence.Sender != null && keySequence.Sender != sender) {
                    continue; // keysequence is not for this sender
                }
                bool sequenceShouldBeExecuted = keySequence.CheckKeyComb(e, sender);
                if(sequenceShouldBeExecuted)
                {
                    keySequence.Action?.Invoke(e, sender);
                    // Reset all key combs' last pressed times to avoid multiple triggers
                    foreach (var keyComb in keySequence.KeyCombs)
                    {
                        keyComb.LastPressedTime = null;
                    }
                    break;
                }
            }
        }

        public void ShowShortCuts() { 
            
        }
    }
}
