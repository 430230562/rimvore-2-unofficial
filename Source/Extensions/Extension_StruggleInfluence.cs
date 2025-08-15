using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    /// <summary>
    /// Can be applied to TraitDef and race ThingDef
    /// </summary>
    public class Extension_StruggleInfluence : DefModExtension
    {
        List<StruggleInfluence> influences = new List<StruggleInfluence>();

        public IEnumerable<StruggleInfluence> InfluencesForRace(VoreRole role) => influences.Where(i => i.role == role);
        public IEnumerable<StruggleInfluence> InfluencesForTraits(VoreRole role, int traitDegree) =>
            influences.Where(i => i.role == role
                && i.traitDegree == traitDegree
        );

        public bool NoStrugglingRace(VoreRole role)
        {
            IEnumerable<StruggleInfluence> applicableInfluences = InfluencesForRace(role);
            return NoStruggling(applicableInfluences);
        }
        public bool NoStrugglingTraits(VoreRole role, int traitDegree)
        {
            IEnumerable<StruggleInfluence> applicableInfluences = InfluencesForTraits(role, traitDegree);
            return NoStruggling(applicableInfluences);
        }
        public float StruggleChanceMultiplierRace(VoreRole role)
        {
            IEnumerable<StruggleInfluence> applicableInfluences = InfluencesForRace(role);
            return StruggleChanceMultiplier(applicableInfluences);
        }
        public float StruggleChanceMultiplierTraits(VoreRole role, int traitDegree)
        {
            IEnumerable<StruggleInfluence> applicableInfluences = InfluencesForTraits(role, traitDegree);
            return StruggleChanceMultiplier(applicableInfluences);
        }
        public float RequiredStrugglesMultiplierRace(VoreRole role)
        {
            IEnumerable<StruggleInfluence> applicableInfluences = InfluencesForRace(role);
            return RequiredStrugglesMultiplier(applicableInfluences);
        }
        public float RequiredStrugglesMultiplierTraits(VoreRole role, int traitDegree)
        {
            IEnumerable<StruggleInfluence> applicableInfluences = InfluencesForTraits(role, traitDegree);
            return RequiredStrugglesMultiplier(applicableInfluences);
        }

        private bool NoStruggling(IEnumerable<StruggleInfluence> influences)
        {
            return influences
                .Any(influence => influence.blockStruggle);
        }

        private float StruggleChanceMultiplier(IEnumerable<StruggleInfluence> influences)
        {
            IEnumerable<float> applicableInfluences = influences
                .Select(influence => influence.struggleChanceMultiplier);
            if(applicableInfluences.EnumerableNullOrEmpty())
            {
                return 1f;
            }
            return applicableInfluences
                .Aggregate((a, b) => a * b);
        }

        private float RequiredStrugglesMultiplier(IEnumerable<StruggleInfluence> influences)
        {
            IEnumerable<float> applicableInfluences = influences
                .Select(influence => influence.requiredStrugglesMultiplier);
            if(applicableInfluences.EnumerableNullOrEmpty())
            {
                return 1f;
            }
            return applicableInfluences
                .Aggregate((a, b) => a * b);
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
                yield return error;
            if(influences.NullOrEmpty())
                yield return $"Required list {nameof(influences)} is empty!";
            foreach(StruggleInfluence influence in influences)
                foreach(string error in influence.ConfigErrors())
                    yield return error;
        }
    }

    public class StruggleInfluence
    {
        public int traitDegree = 0;
        public VoreRole role;
        public float struggleChanceMultiplier = 1f;
        public float requiredStrugglesMultiplier = 1f;
        public bool blockStruggle = false;

        public IEnumerable<string> ConfigErrors()
        {
            if(role == VoreRole.Invalid)
                yield return $"Required field {nameof(role)} was not set";
        }
    }
}
