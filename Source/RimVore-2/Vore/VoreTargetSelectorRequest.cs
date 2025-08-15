using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class VoreTargetSelectorRequest : IExposable
    {
        public VoreGoalDef voreGoal = null;
        public VoreTypeDef voreType = null;
        public VoreRole role = VoreRole.Invalid;
        public RaceType raceType = RaceType.Invalid;
        public Gender gender = Gender.None;
        public bool allMustMatch = false;

        public VoreTargetSelectorRequest() { }

        public VoreTargetSelectorRequest(bool allMustMatch)
        {
            this.allMustMatch = allMustMatch;
        }

        public bool Matching(VoreTargetSelectorRequest request)
        {
            if(request.allMustMatch)
            {
                return AllMatching(request);
            }
            else
            {
                return AnyMatching(request);
            }
        }

        private bool AllMatching(VoreTargetSelectorRequest request)
        {
            IEnumerable<bool> matches = Matching(request, true);
            //Log.Message("all-match, request / this:\n" + request.ToString() + "\n" + this.ToString() + "\n" + string.Join(":", matches));
            return matches.All(m => m == true);
        }
        private bool AnyMatching(VoreTargetSelectorRequest request)
        {
            IEnumerable<bool> matches = Matching(request, false);
            //Log.Message("any-match, request / this:\n" + request.ToString() + "\n" + this.ToString() + "\n" + string.Join(":", matches));
            return matches.Any(m => m == true);
        }
        private IEnumerable<bool> Matching(VoreTargetSelectorRequest request, bool defaultMatchIsValid)
        {
            if(defaultMatchIsValid && request.voreGoal == default(VoreGoalDef) && voreGoal == default(VoreGoalDef))
            {
                //Log.Message("yielding goal default match");
                yield return true;
            }
            else
            {
                bool match = voreGoal == request.voreGoal;
                //Log.Message("yielding goal match " + match);
                yield return match;

            }
            if(defaultMatchIsValid && request.voreType == default(VoreTypeDef) && voreType == default(VoreTypeDef))
            {
                //Log.Message("yielding type default match");
                yield return true;
            }
            else
            {
                bool match = voreType == request.voreType;
                //Log.Message("yielding type match " + match);
                yield return match;

            }
            if(defaultMatchIsValid && request.role == default(VoreRole) && role == default(VoreRole))
            {
                //Log.Message("yielding role default match");
                yield return true;
            }
            else
            {
                bool match = role == request.role;
                //Log.Message("yielding role match " + match);
                yield return match;

            }
            if(defaultMatchIsValid && request.raceType == default(RaceType) && raceType == default(RaceType))
            {
                //Log.Message("yielding racetype default match");
                yield return true;
            }
            else
            {
                bool match = raceType == request.raceType;
                //Log.Message("yielding racetype match " + match);
                yield return match;
            }
            if(defaultMatchIsValid && request.gender == default(Gender) && gender == default(Gender))
            {
                yield return true;
            }
            else
            {
                yield return gender == request.gender;
            }
            yield break;
        }

        public IEnumerable<string> ConfigErrors()
        {
            if(voreGoal == null && voreType == null && role == VoreRole.Invalid && raceType == RaceType.Invalid && gender == Gender.None)
            {
                yield return $"At least one field is required: \"{nameof(voreGoal)}\", \"{nameof(voreType)}\", \"{nameof(role)}\", \"{nameof(raceType)}\" or \"{nameof(gender)}\"";
            }
        }

        public string FullToString()
        {
            return "voreGoal: " +
                voreGoal +
                " | voreType: " +
                voreType +
                " | role: " +
                role +
                " | raceType: " + raceType;
        }

        public override string ToString()
        {
            List<string> strings = new List<string>();
            if(voreGoal != default(VoreGoalDef))
            {
                strings.Add("voreGoal: " + voreGoal);
            }
            if(voreType != default(VoreTypeDef))
            {
                strings.Add("voreType: " + voreType);
            }
            if(role != default(VoreRole))
            {
                strings.Add("role: " + role);
            }
            if(raceType != default(RaceType))
            {
                strings.Add("raceType: " + raceType);
            }
            if(strings.NullOrEmpty())
            {
                return "EMPTY";
            }
            return string.Join(" | ", strings);
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref voreGoal, "voreGoal");
            Scribe_Defs.Look(ref voreType, "voreType");
            Scribe_Values.Look(ref role, "role");
            Scribe_Values.Look(ref raceType, "raceType");
        }
    }
}
