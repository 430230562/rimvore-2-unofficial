using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class RollPresetDef : Def
    {
        public RollPresetDef() : base() { }

        public bool canBeBlockedByAction = false;

        public int interval = 1;
        public float successChance = 1f;
        public List<RollModifier> chanceModifiers = new List<RollModifier>();
        public FloatRange strength = new FloatRange(0, 0);
        public List<RollModifier> strengthModifiers = new List<RollModifier>();

        public List<RollAction> actionsOnSuccess = new List<RollAction>();
        public List<RollAction> actionsOnFailure = new List<RollAction>();

        public override IEnumerable<string> ConfigErrors()
        {
            foreach(string error in base.ConfigErrors())
            {
                yield return error;
            }
            foreach(RollModifier modifier in strengthModifiers)
            {
                foreach(string error in modifier.ConfigErrors())
                {
                    yield return error;
                }
            }
            foreach(RollModifier modifier in chanceModifiers)
            {
                foreach(string error in modifier.ConfigErrors())
                {
                    yield return error;
                }
            }
            foreach(RollAction action in actionsOnSuccess)
            {
                foreach(string error in action.ConfigErrors())
                {
                    yield return error;
                }
            }
            foreach(RollAction action in actionsOnFailure)
            {
                foreach(string error in action.ConfigErrors())
                {
                    yield return error;
                }
            }
        }
    }
}
