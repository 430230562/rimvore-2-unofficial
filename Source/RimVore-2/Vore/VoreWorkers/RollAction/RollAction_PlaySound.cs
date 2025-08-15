using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class RollAction_PlaySound : RollAction
    {
        SoundDef sound;

        public RollAction_PlaySound() : base()
        {
            target = VoreRole.Predator;
        }

        public override bool TryAction(VoreTrackerRecord record, float rollStrength)
        {
            base.TryAction(record, rollStrength);
            if(!TargetPawn.Spawned || sound == null)
            {
                return false;
            }
            SoundManager.PlaySingleSound(TargetPawn, sound);
            return true;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(sound == null)
            {
                yield return "Required field \"sound\" not set";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref sound, "sound");
        }
    }
}
