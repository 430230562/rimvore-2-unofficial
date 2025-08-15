using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RimVore2
{
    /// One thing to keep in mind is that skills use exponentially more experience points per level, 
    ///   trying to steal someones level whilst at lvl19 will take way more giver skills to 
    ///   sum up the experience points necessary for the takers skill upgrade
    ///   
    public class RollAction_StealSkill : RollAction_StealComparable<SkillDef>
    {
        SkillDef skill;
        public override SkillDef FixedSelector => skill;

        public override IEnumerable<SkillDef> SelectionRetrieval(Pawn pawn)
        {
            IEnumerable<SkillDef> skills = pawn.skills?.skills?
                .Where(skill => !skill.TotallyDisabled)
                .Select(skill => skill.def);
            if(skills == null)
                return new List<SkillDef>();
            return skills;
        }

        public override float ValueRetrieval(Pawn pawn, SkillDef obj)
        {
            return pawn.skills.GetSkill(obj).Level;

        }

        public override bool TryAction(VoreTrackerRecord record, float rollStrength)
        {
            base.TryAction(record, rollStrength);
            if(TakerPawn.Dead)
            {
                return false;
            }
            List<SkillRecord> takerSkills = TakerPawn?.skills?.skills;
            if(takerSkills.NullOrEmpty())
            {
                RV2Log.Warning("Can't steal skills for taker with no skills: " + TakerPawn?.Label);
                return false;
            }
            List<SkillRecord> giverSkills = GiverPawn?.skills?.skills;
            if(giverSkills.NullOrEmpty())
            {
                RV2Log.Warning("Can't steal skills from giver with no skills: " + GiverPawn?.Label);
                return false;
            }
            SkillDef skillDef = Choose();
            if(skillDef == null)
            {
                return false;
            }
            if(RV2Log.ShouldLog(true, "OngoingVore"))
                RV2Log.Message($"{TakerPawn.LabelShort} chose skillDef {skillDef.defName} to steal from {GiverPawn.LabelShort}", true, "OngoingVore");
            StealLevels(GiverPawn, TakerPawn, skillDef, rollStrength);
            return true;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            if(useFixed && skill == null)
            {
                yield return "Field \"skill\" is required if field \"useFixed\" is used";
            }
        }

        private void StealLevels(Pawn giver, Pawn taker, SkillDef targetSkill, float amount)
        {
            SkillRecord giverSkill = giver.skills.GetSkill(targetSkill);
            SkillRecord takerSkill = taker.skills.GetSkill(targetSkill);
            // okay, this is a bit complicated due to the exponential growth and base games horrendous tracking of XP, bear with me
            float stolenExperience = 0;

            // we start with stealing the in-progress experience
            float currentProgress = giverSkill.xpSinceLastLevel / giverSkill.XpRequiredForLevelUp;
            if(RV2Log.ShouldLog(true, "OngoingVore"))
                RV2Log.Message($"Stealing {amount} levels. {giver.LabelShort} current level progress: {currentProgress}, taking from that first", false, "OngoingVore");
            if(currentProgress <= 0)
            {
                // uuh, apparently this can happen, don't do anything I guess
            }
            else if(currentProgress > amount)
            {
                // simple case, we don't steal a full level, but "in progress" experience and that's it
                stolenExperience += amount * giverSkill.xpSinceLastLevel;
                giverSkill.xpSinceLastLevel -= stolenExperience;
                amount = 0;
            }
            else
            {
                // we take all XP in the currently progressing level
                amount -= currentProgress;
                stolenExperience += giverSkill.xpSinceLastLevel;
                giverSkill.xpSinceLastLevel = 0;
            }

            // now we take all the full levels
            int infiniteLoopPrevention = 20;    // I hate using while loops, these are mandatory in them in my opinion
            while(amount > 1 && infiniteLoopPrevention-- > 0)
            {
                // first we drop a full level
                giverSkill.levelInt--;
                amount--;
                // we can now use the required XP as the number of XP that we stole with the full level drop
                stolenExperience += giverSkill.XpRequiredForLevelUp;
            }

            // and finally wrap up the steal by emulating a new in-progress skill
            if(amount > 0)
            {
                // we need to drop ANOTHER level, and then add the remaining XP, dropping 0.2 means we drop a full level and fill XP by 80%
                giverSkill.levelInt--;
                giverSkill.xpSinceLastLevel = (1 - amount) * giverSkill.XpRequiredForLevelUp;
                stolenExperience += amount * giverSkill.XpRequiredForLevelUp;
                amount = 0;
            }

            if(RV2Log.ShouldLog(true, "OngoingVore"))
                RV2Log.Message($"Stole a total of {stolenExperience} experience", false, "OngoingVore");
            if(!RV2Mod.Settings.cheats.TraitStealIgnoresLearningFactor)
            {
                float learningFactor = takerSkill.LearnRateFactor(true);
                stolenExperience *= learningFactor;
                if(RV2Log.ShouldLog(true, "PostVore"))
                    RV2Log.Message($"Modified the stolen experience with learning factor of {learningFactor} for a new total experience of {stolenExperience}", false, "OngoingVore");
            }
            takerSkill.Learn(stolenExperience, true);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref skill, "skill");
        }
    }
}
