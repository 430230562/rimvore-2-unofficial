using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{

    /// <summary>
    /// This extension can be applied to either traits or a race ThingDef
    /// </summary>
    public class Extension_GrappleInfluence : DefModExtension
    {
        List<GrappleInfluence> influences = new List<GrappleInfluence>();

        private IEnumerable<GrappleInfluence> InfluencesForRace(GrappleRole role) =>
            influences.Where(influence => influence.role == role);
        private IEnumerable<GrappleInfluence> InfluencesForTraits(GrappleRole role, int traitDegree) =>
            influences.Where(influence => influence.role == role && influence.traitDegree == traitDegree);

        public float StrengthMultiplierForRace(GrappleRole role)
        {
            IEnumerable<GrappleInfluence> applicableInfluences = InfluencesForRace(role);
            return StrengthMultiplier(applicableInfluences);
        }
        public float StrengthMultiplierForTraits(GrappleRole role, int degree)
        {
            IEnumerable<GrappleInfluence> applicableInfluences = InfluencesForTraits(role, degree);
            return StrengthMultiplier(applicableInfluences);
        }
        public float ChanceMultiplierForRace(GrappleRole role)
        {
            IEnumerable<GrappleInfluence> applicableInfluences = InfluencesForRace(role);
            return ChanceMultiplier(applicableInfluences);
        }
        public float ChanceMultiplierForTraits(GrappleRole role, int degree)
        {
            IEnumerable<GrappleInfluence> applicableInfluences = InfluencesForTraits(role, degree);
            return ChanceMultiplier(applicableInfluences);
        }

        private float StrengthMultiplier(IEnumerable<GrappleInfluence> influences)
        {
            IEnumerable<float> applicableInfluences = influences
                .Select(influence => influence.strengthMultiplier);
            if(applicableInfluences.EnumerableNullOrEmpty())
            {
                return 1f;
            }
            return applicableInfluences
                .Aggregate((a, b) => a * b);
        }
        private float ChanceMultiplier(IEnumerable<GrappleInfluence> influences)
        {
            IEnumerable<float> applicableInfluences = influences
                .Select(influence => influence.chanceMultiplier);
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
                yield return $"Required list {nameof(influences)} is empty";
            foreach(GrappleInfluence influence in influences)
                foreach(string error in influence.ConfigErrors())
                    yield return error;
        }
    }

    public class GrappleInfluence
    {
        public int traitDegree = 0;
        public GrappleRole role;
        public float strengthMultiplier = 1f;
        public float chanceMultiplier = 1f;

        public IEnumerable<string> ConfigErrors()
        {
            if(role == GrappleRole.Invalid)
                yield return $"Required field {nameof(role)} is not set";
        }
    }
}
