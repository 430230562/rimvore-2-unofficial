using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2.Patches
{
    [HarmonyPatch(typeof(InteractionUtility), "TryGetRandomVerbForSocialFight")]
    public class Patch_InteractionUtility
    {
        [HarmonyPostfix]
        public static void PreventGrappleVerbSelection(Pawn p, ref Verb verb)
        {
            var capacities = verb?.tool?.capacities;
            if(capacities == null || !capacities.Contains(RV2_Common.VoreGrappleToolCapacity))
            {
                return;
            }

            verb = p.verbTracker.AllVerbs
                .Where(v => IsValid(v))
                .RandomElementByWeightWithFallback(v => GetWeight(v));

            bool IsValid(Verb v)
            {
                if(!v.IsMeleeAttack)
                {
                    return false;
                }
                if(!v.IsStillUsableBy(p))
                {
                    return false;
                }
                // filter out grapple
                if(v.tool.capacities.Contains(RV2_Common.VoreGrappleToolCapacity))
                { 
                    return false;
                }
                return true;
            }

            // originates from base game, rewritten for readability
            // original: .TryRandomElementByWeight((Verb x) => x.verbProps.AdjustedMeleeDamageAmount(x, p) * ((x.tool != null) ? x.tool.chanceFactor : 1f), out verb);
            float GetWeight(Verb v)
            {
                float damage = v.verbProps.AdjustedMeleeDamageAmount(v, p);
                if(v.tool != null)
                {
                    damage *= v.tool.chanceFactor;
                }
                return damage;
            }
        }
    }
}
