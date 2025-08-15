using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{

    public class RV2DesignationDef : Def
    {
        public RuleTargetRole assignedTo;
        public bool lethal = false;
        public string iconPathEnabledManually;
        public string iconPathDisabledManually;
        public string iconPathEnabledAutomatically;
        public string iconPathDisabledAutomatically;
        public string iconPathBlocked;
        public string tipEnabledManually;
        public string tipDisabledManually;
        public string tipEnabledAutomatically;
        public string tipDisabledAutomatically;
        public string tipBlocked;

        public bool IsEnabledFor(Pawn pawn, out string reason)
        {
            bool enabled = pawn.PawnData()?.Designations?.TryGetValue(this)?.IsEnabled() == true;
            if(!enabled)
            {
                reason = "RV2_VoreInvalidReasons_DesignationMissing".Translate(pawn.Label, label);
                return false;
            }
            reason = null;
            return true;
        }

        public bool IsEnabledFor(Pawn predator, Pawn prey, out string reason)
        {
            if(assignedTo == RuleTargetRole.All || assignedTo == RuleTargetRole.Predator)
            {
                if(!IsEnabledFor(predator, out reason))
                {
                    return false;
                }
            }
            if(assignedTo == RuleTargetRole.All || assignedTo == RuleTargetRole.Prey)
            {
                if(!IsEnabledFor(prey, out reason))
                {
                    return false;
                }
            }

            reason = null;
            return true;
        }
        public bool AppliesToRole(RuleTargetRole role)
        {
            if(role == RuleTargetRole.All || assignedTo == RuleTargetRole.All)
                return true;
            return assignedTo == role;
        }

        public Texture2D CurrentIcon(Pawn pawn)
        {
            RV2Designation designation = pawn.PawnData()?.Designations.SingleOrDefault(kvp => kvp.Key == this).Value;
            return CurrentIcon(designation);
        }
        public Texture2D CurrentIcon(RV2Designation designation)
        {
            Pawn pawn = designation.pawn;
            string path;
            if(IsBlocked(pawn)) path = iconPathBlocked;
            else if(designation.isSetManually)
            {
                if(designation.enabledManual) path = iconPathEnabledManually;
                else path = iconPathDisabledManually;
            }
            else
            {
                if(designation.enabledAuto) path = iconPathEnabledAutomatically;
                else path = iconPathDisabledAutomatically;
            }

            return ContentFinder<Texture2D>.Get(path);
        }

        public string CurrentTip(RV2Designation designation)
        {
            if(designation == null)
            {
                return null;
            }
            Pawn pawn = designation.pawn;
            if(IsBlocked(pawn)) return tipBlocked;
            else if(designation.isSetManually)
            {
                if(designation.enabledManual) return tipEnabledManually;
                else return tipDisabledManually;
            }
            else
            {
                if(designation.enabledAuto) return tipEnabledAutomatically;
                else return tipDisabledAutomatically;
            }
        }

        public bool IsBlocked(Pawn pawn)
        {
            QuirkManager pawnQuirks = pawn.QuirkManager(false);
            if(pawnQuirks == null)
            {
                return false;
            }
            return pawnQuirks.HasDesignationBlock(this);
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(iconPathEnabledManually == null)
            {
                yield return "Required field \"iconPathEnabledManually\" not set";
            }
            if(iconPathDisabledManually == null)
            {
                yield return "Required field \"iconPathDisabledManually\" not set";
            }
            if(iconPathEnabledAutomatically == null)
            {
                yield return "Required field \"iconPathEnabledAutomatically\" not set";
            }
            if(iconPathDisabledAutomatically == null)
            {
                yield return "Required field \"iconPathDisabledAutomatically\" not set";
            }
            if(iconPathBlocked == null)
            {
                yield return "Required field \"iconPathBlocked\" not set";
            }
            if(tipEnabledManually == null)
            {
                yield return "Required field \"tipEnabledManually\" not set";
            }
            if(tipDisabledManually == null)
            {
                yield return "Required field \"tipDisabledManually\" not set";
            }
            if(tipEnabledAutomatically == null)
            {
                yield return "Required field \"tipEnabledAutomatically\" not set";
            }
            if(tipDisabledAutomatically == null)
            {
                yield return "Required field \"tipDisabledAutomatically\" not set";
            }
            if(tipBlocked == null)
            {
                yield return "Required field \"tipBlocked\" not set";
            }
        }
    }
}
