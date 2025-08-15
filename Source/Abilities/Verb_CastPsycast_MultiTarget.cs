using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimVore2
{
    public class Verb_CastPsycast_MultiTarget : Verb_CastAbility
    {
        protected override bool TryCastShot()
        {
            if(!base.TryCastShot())
                return false;
            Ability_MultiTarget multiAbility = ability as Ability_MultiTarget;
            if(multiAbility == null)
            {
                Log.Error("Verb_CastPsycast_MultiTarget on ability that is not Ability_MultiTarget");
                return false;
            }
            multiAbility.initialTarget = null;
            multiAbility.targets.Clear();
            AbilityExtension_MultiTarget multiTargetExtension = multiAbility.def.GetModExtension<AbilityExtension_MultiTarget>();
            if(multiTargetExtension == null)
            {
                Log.Error("AbilityExtension_MultiTarget missing");
                return false;
            }
            int requiredTargets = multiTargetExtension.requiredTargets - 1;  // first target is picked natively by the base.TryCastShot()

            multiAbility.initialTarget = currentTarget;
            multiAbility.targets.Add(currentTarget);

            TargetingParameters targetingParameters = AdditionalTargetParameters(multiAbility);
            if(targetingParameters == null)
                return false;
            BeginTargeting(targetingParameters, multiAbility, requiredTargets);
            return true;
        }

        protected virtual TargetingParameters AdditionalTargetParameters(Ability_MultiTarget multiAbility)
        {
            TargetingParameters additionalTargetParameters = targetParams.Clone();
            if(additionalTargetParameters == null)
            {
                Log.Error("Could not retrieve TargetParameters");
                return null;
            }
            List<Predicate<TargetInfo>> predicates = new List<Predicate<TargetInfo>>();
            additionalTargetParameters.validator = (TargetInfo target) =>
                predicates.All(predicate => predicate(target));

            if(multiAbility.AdditionalTargetValidator != null)
                predicates.Add(multiAbility.AdditionalTargetValidator);
            predicates.Add((TargetInfo target) =>
            {
                if(!multiAbility.CanApplyOn(target))
                    return false;
                // base game spams messages if we directly call ValidateTarget(), so we need to call the copied one that actually doesn't ignore the "showMessages" parameter
                return ValidateTargetWithBlockedMessages(target.Thing, false);
            });

            // no need to null-check the GetModExtension, calling method already null-checked
            if(!multiAbility.def.GetModExtension<AbilityExtension_MultiTarget>().canPickSameTargetMultipleTimes)
            {
                predicates.Add((TargetInfo otherTarget) =>
                    !multiAbility.targets.Any(target => target.Pawn == otherTarget.Thing)
                );
            }
            return additionalTargetParameters;
        }

        /// <summary>
        /// carbon copy of base game Verb_CastAbility.ValidateTarget
        /// </summary>
        public bool ValidateTargetWithBlockedMessages(LocalTargetInfo target, bool showMessages = true)
        {
            if(this.verbProps.range > 0f)
            {
                if(!this.CanHitTarget(target))
                {
                    if(target.IsValid)
                    {
                        if(showMessages)
                        {
                            Messages.Message(this.ability.def.LabelCap + ": " + "AbilityCannotHitTarget".Translate(), new LookTargets(new TargetInfo[]
                            {
                            this.ability.pawn,
                            target.ToTargetInfo(this.ability.pawn.Map)
                            }), MessageTypeDefOf.RejectInput, false);
                        }
                    }
                    return false;
                }
            }
            else if(!this.ability.pawn.CanReach(target, PathEndMode.Touch, this.ability.pawn.NormalMaxDanger(), mode: TraverseMode.ByPawn))
            {
                if(target.IsValid)
                {
                    if(showMessages)
                    {

                        Messages.Message(this.ability.def.LabelCap + ": " + "AbilityCannotReachTarget".Translate(), new LookTargets(new TargetInfo[]
                        {
                        this.ability.pawn,
                        target.ToTargetInfo(this.ability.pawn.Map)
                        }), MessageTypeDefOf.RejectInput, false);
                    }
                }
                return false;
            }
            if(!this.IsApplicableTo(target, showMessages))
                return false;
            for(int i = 0; i < this.ability.EffectComps.Count; i++)
            {
                if(!this.ability.EffectComps[i].Valid(target, showMessages))
                    return false;
            }
            return true;
        }

        private void BeginTargeting(TargetingParameters targetParameters, Ability_MultiTarget multiAbility, int requiredTargets)
        {
            Targeter_ForcePause.Targeter.UpdateAction = () =>
            {
                DrawHighlight(null);
            };
            Targeter_ForcePause.Targeter.BeginTargeting(targetParameters, multiAbility as ITargetingSource, (LocalTargetInfo target) =>
            {
                multiAbility.targets.Add(target);
                if(multiAbility.targets.Count >= requiredTargets)
                    FinishTargeting(multiAbility);
                else
                    BeginTargeting(targetParameters, multiAbility, requiredTargets);
            });
        }

        private void FinishTargeting(Ability_MultiTarget multiAbility)
        {
            Targeter_ForcePause.Targeter.StopTargeting();
            multiAbility.Notify_AllTargetsPicked();
        }
    }
}