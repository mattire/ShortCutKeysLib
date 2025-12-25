using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static ShortCutKeysLib.KeySequenceMngr;
using KeyCombSeq = ShortCutKeysLib.KeyCombSequence;

namespace ShortCutKeysLib
{
    public class KeySeqStateManager
    {
        public KeySeqStateManager(string storePath)
        {
            StorePath = storePath;
        }

        public string StorePath { get; }

        public void LoadState(KeySequenceMngr keySequenceMngr, string? alternatePath = null)
        {
            string loadPath = alternatePath ?? StorePath;

            if (!File.Exists(loadPath)) { return; }
            var txt = File.ReadAllText(loadPath);

            List<KeyCombSeq>? deser = JsonSerializer.Deserialize<List<KeyCombSeq>>(txt);
            if (deser != null)
            {
                foreach (var kcs in keySequenceMngr.KeyCombSequences)
                {
                    var deserKcs = deser.FirstOrDefault(d => d.Name != null && d.Name.Equals(kcs.Name, StringComparison.InvariantCultureIgnoreCase));
                    if (deserKcs != null)
                    { kcs.KeyCombs = deserKcs.KeyCombs; }
                }
            }
        }

        public void SaveState(KeySequenceMngr keySequenceMngr, string? alternatePath = null)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    WriteIndented = true
                };

                string ser = JsonSerializer.Serialize(keySequenceMngr.KeyCombSequences, options);

                if (alternatePath != null)
                {
                    File.WriteAllText(alternatePath, ser);
                    return;
                }
                File.WriteAllText(StorePath, ser);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine( e);
            }        
        }
    }
}
