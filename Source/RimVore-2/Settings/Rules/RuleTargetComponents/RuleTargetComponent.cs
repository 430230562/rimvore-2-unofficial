using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public abstract class RuleTargetComponent : ICloneable, IExposable
    {
        /// <summary>
        /// Do not call .Translate() on this key, they must be used by DefsLoaded to properly initialize
        /// </summary>
        public abstract string ButtonTranslationKey { get; }
        public abstract string PawnExplanation(Pawn pawn);

        public RuleTargetRole TargetRole;
        protected bool inverted = false;
        public RuleTargetComponent() { }
        public RuleTargetComponent(RuleTargetRole sourceRole) : this()
        {
            this.TargetRole = sourceRole;
        }

        public abstract RuleTargetStaleTrigger MakeStaleTrigger();
        public abstract string Label { get; }
        public bool AppliesToPawn(Pawn pawn)
        {
            bool appliesTo = AppliesToPawnInteral(pawn);
            if(inverted)
                return !appliesTo;
            return appliesTo;
        }
        protected abstract bool AppliesToPawnInteral(Pawn pawn);
        public bool AppliesToRole(RuleTargetRole role)
        {
            if(role == RuleTargetRole.All || TargetRole == RuleTargetRole.All)
                return true;    // ignore inversion in this case
            bool applies = TargetRole == role;

            if(inverted)
                return !applies;
            return applies;
        }
        public abstract object Clone();

        static Func<RuleTargetRole, string> ruleTargetRolePresenter = (RuleTargetRole role) => $"RV2_RuleTargetRole_{role}".Translate();
        public virtual void DrawInteractible(Listing_Standard list)
        {
            Action<RuleTargetRole> setter = (RuleTargetRole newRole) => TargetRole = newRole;
            list.EnumLabeled("RV2_Settings_Rules_IdentifierRole".Translate(), TargetRole, setter, ruleTargetRolePresenter);
            list.CheckboxLabeled("RV2_Settings_Rule_RuleTargetComponent_Inverted".Translate(), ref inverted, "RV2_Settings_Rule_RuleTargetComponent_Inverted".Translate());
            DrawInteractibleInternal(list);
        }
        public virtual bool RequiresRoyalty => false;
        public virtual bool RequiresIdeology => false;
        public virtual bool RequiresBiotech => false;
        public abstract void DrawInteractibleInternal(Listing_Standard list);
        public virtual void DefsLoaded()
        {
            if(!IsValid())
            {
                SetFallback(out string warningMessage);
                Log.Warning(warningMessage);
            }
        }
        /// <summary>
        /// Used to validate that a given component is still valid, if not, <see cref="SetFallback(out string)"/> will be called
        /// </summary>
        public abstract bool IsValid();
        /// <summary>
        /// Called if the current Target is no longer valid due to unloaded Defs. 
        /// </summary>
        /// <param name="message">Message to log out as warning for user to read</param>
        public abstract void SetFallback(out string message);
        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref TargetRole, "targetRole");
            Scribe_Values.Look(ref inverted, "inverted");
        }
    }
}
