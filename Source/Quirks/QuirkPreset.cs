using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    /// <remarks>
    /// Presets are tracked in the SettingsContainer_Quirks, which allows for cross-game quirk presets
    /// </remarks>
    public class QuirkPreset : IExposable
    {
        private List<string> scribedQuirks;

        public QuirkPreset()
        {
            if(Scribe.mode == LoadSaveMode.Inactive)
            {
                Log.Warning($"Called base constructor outside of scribing, this will cause issues");
            }
        }

        public QuirkPreset(List<QuirkDef> quirks)
        {
            _quirks = quirks;
            scribedQuirks = quirks.Select(def => def.defName).ToList();
        }

        private List<QuirkDef> _quirks;
        public List<QuirkDef> Quirks
        {
            get
            {
                if(_quirks == null)
                {
                    _quirks = new List<QuirkDef>();
                    foreach(string defName in scribedQuirks)
                    {
                        _quirks.Add(DefDatabase<QuirkDef>.GetNamed(defName));
                    }
                }
                return _quirks;
            }
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref scribedQuirks, nameof(scribedQuirks), LookMode.Value);
        }
    }
}
