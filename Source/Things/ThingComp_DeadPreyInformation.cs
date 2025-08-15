using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class ThingCompProperties_DeadPreyInformation : CompProperties
    {
        public ThingCompProperties_DeadPreyInformation() : base()
        {
            compClass = typeof(ThingComp_DeadPreyInformation);
        }
    }

    public class ThingComp_DeadPreyInformation : ThingComp
    {
        public string ExtraLabel;
        public string ExtraDescription;

        public void CopyFrom(ThingComp_DeadPreyInformation parent)
        {
            this.ExtraLabel = parent.ExtraLabel;
            this.ExtraDescription = parent.ExtraDescription;
        }

        public void Init(Pawn prey)
        {
            if(prey == null)
            {
                return;
            }
            try
            {
                NamedArgument argName = prey.NameFullColored.Named("NAME");
                NamedArgument argAge = prey.ageTracker.AgeBiologicalYears.Named("AGE");
                string profession;
                if(prey.IsHumanoid() && prey.story != null)
                {
                    profession = prey.story.TitleCap ?? "RV2_ExtraDescription_VoreProductContainer_ProfessionFallback".Translate();
                }
                else
                {
                    // animals or mechanoids are considered by their kind, so it will show "wolf of FACTION"
                    profession = prey.KindLabel;
                }
                NamedArgument argProfession = profession.Named("PROFESSION");
                NamedArgument argFaction = prey.Faction.Named("FACTION");
                ExtraLabel = "RV2_ExtraDescription_VoreProductContainer_Label".Translate(argName);
                ExtraDescription = "RV2_ExtraDescription_VoreProductContainer".Translate(argName, argAge, argProfession, argFaction);
            }
            catch(Exception e)
            {
                RV2Log.Warning("Could not create prey details for VoreProductContainer: " + e);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ExtraLabel, "extraLabel");
            Scribe_Values.Look(ref ExtraDescription, "extraDescription");
        }

        public override string ToString()
        {
            return $"{base.ToString()} - ExtraLabel: {ExtraLabel} - ExtraDescription: {ExtraDescription}";
        }
    }
}
