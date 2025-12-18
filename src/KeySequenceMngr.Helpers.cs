using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ShortCutKeysLib
{
    public partial class KeySequenceMngr
    {
        public void AddSequences(List<KeyCombSequence> newkeyCombSeqs,
                                 bool overrideSeqs = true) 
        {
            List<string> allreadyExist = new List<string>();

            foreach (var kcs in newkeyCombSeqs) {
                if (!KeyCombSequences.Any(k => k.Name == kcs.Name)) { 
                    KeyCombSequences.Add(kcs);
                } else { 
                    allreadyExist.Add(kcs.Name);
                    if(overrideSeqs) {
                        var existingKcs = KeyCombSequences.First(k => k.Name == kcs.Name);
                        KeyCombSequences.Remove(existingKcs);
                        KeyCombSequences.Add(kcs);
                    }
                }
            }
            if(overrideSeqs) { ShowOverridden(allreadyExist); }
            else             { ShowNotAdded(allreadyExist); }
        }

        public void AddControlSequences(
            List<(List<System.Windows.Input.Key> keys, Action<KeyEventArgs, object> act, object? sender, string name, string desc)> controlKeyShortCuts, 
            bool overrideSeqs = true) 
        {
            List<KeyCombSequence> controlKeyCombSeqs = new List<KeyCombSequence>();
            foreach (var controlSeq in controlKeyShortCuts) 
            {
                var kcCeq = new KeyCombSequence(controlSeq.name, controlSeq.desc, new List<KeyComb>(), controlSeq.act, controlSeq.sender );
                foreach (var k in controlSeq.keys) { 
                    
                    var kc = new KeyComb() { Key = k, Mods = System.Windows.Input.ModifierKeys.Control };
                    kcCeq.KeyCombs.Add(kc);
                }

                controlKeyCombSeqs.Add(kcCeq);
            }
            AddSequences(controlKeyCombSeqs, overrideSeqs);
        }

        internal void SaveState()
        {
            StateManager.SaveState(this);
        }

        internal void SaveToFile(string filename)
        {
            StateManager.SaveState(this, filename);
        }

        internal void LoadFromFile(string filename)
        {
            StateManager.LoadState(this, filename);
        }
    }
}
