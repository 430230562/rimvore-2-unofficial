using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public static class VoreFeedUtility
    {
        public static TargetingParameters PredatorParameters(Pawn feeder, Pawn prey)
        {
            TargetingParameters targetingParameters = new TargetingParameters
            {
                canTargetBuildings = false,
                validator = delegate (TargetInfo targ)
                {
                    if(!(targ.Thing is Pawn predator))
                    {
                        return false;
                    }
                    if(predator == prey)
                    {
                        return false;
                    }
                    if(predator == feeder)
                    {
                        return false;
                    }
                    if(!feeder.CanReach(predator, Verse.AI.PathEndMode.Touch, Danger.Deadly))
                    {
                        return false;
                    }
                    if(!predator.CanVore(prey, out string reason))
                    {
                        return false;
                    }
                    return true;
                }
            };
            return targetingParameters;
        }

        public static bool CanFeedToOthers(Pawn pawn, out string reason)
        {
            if(pawn.Dead)
            {
                reason = "RV2_VoreInvalidReasons_PawnDead".Translate();
                return false;
            }
            if(!pawn.CanBePrey(out reason))
            {
                return false;
            }
            reason = null;
            return true;
        }
    }
}
