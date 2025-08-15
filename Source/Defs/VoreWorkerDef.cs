//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Verse;

//namespace RimVore2
//{
//    public class VoreWorkerDef : Def
//    {
//        public override IEnumerable<string> ConfigErrors()
//        {
//            foreach (string error in base.ConfigErrors())
//            {
//                yield return error;
//            }
//            if(workerClass == null)
//            {
//                yield return "Required field \"workerClass\" is not provided";
//            }
//        }

//        private VoreWorker voreWorker;
//        public VoreWorker VoreWorker
//        {
//            get
//            {
//                if(voreWorker == null)
//                {
//                    voreWorker = (VoreWorker)Activator.CreateInstance(workerClass, this);
//                    //voreWorker.def = this;
//                }
//                return voreWorker;
//            }
//        }

//        /*public int rollIntervalRareTicks = 1;
//        public float rollChance = 1f;
//        public float minimumRollStrength = 1f;
//        public float maximumRollStrength = 1f;*/
//        public Type workerClass;

//        /*#pragma warning disable CS0649 // suppress "Never is assigned to" warning
//        private readonly List<VorePassValue> passValues;
//        #pragma warning disable CS0649
//        public List<VorePassValue> PassValues
//        {
//            get
//            {
//                if(passValues == null)
//                {
//                    return new List<VorePassValue>();
//                }
//                return passValues;
//            }
//        }

//        public List<VorePassValue> PassValuesOnStart => PassValues.FindAll(passValue => passValue.timing == VorePassValueTiming.OnStart);
//        public List<VorePassValue> PassValuesOnEnd=> PassValues.FindAll(passValue => passValue.timing == VorePassValueTiming.OnEnd);
//        public List<VorePassValue> PassValuesAlways => PassValues.FindAll(passValue => passValue.timing == VorePassValueTiming.Always);
//        public List<VorePassValue> PassValuesOnRoll => PassValues.FindAll(passValue => passValue.timing == VorePassValueTiming.OnRoll);*/
//    }

//    public class VoreWorkerDef_Damage : VoreWorkerDef
//    {
//        public DamageDef appliedDamageDef;

//        public override IEnumerable<string> ConfigErrors()
//        {
//            foreach (string error in base.ConfigErrors())
//            {
//                yield return error;
//            }
//            if(appliedDamageDef == null)
//            {
//                yield return "required field \"appliedDamageDef\" not provided";
//            }
//        }
//    }

//    public class VoreWorkerDef_ProduceItem : VoreWorkerDef
//    {
//        public List<ThingDef> items;
//        public float requiredNutrition = 0;
//        public bool subtractsFromPreyNutrition = true;
//        public int quantityPerInterval = 1;
//        public bool itemsDeterminedByContainer = false;

//        public override IEnumerable<string> ConfigErrors()
//        {
//            foreach (string error in base.ConfigErrors())
//            {
//                yield return error;
//            }
//            if(itemsDeterminedByContainer == false && (items == null || items.Count == 0))
//            {
//                yield return "required list \"items\" not provided or empty";
//            }
//        }
//    }

//    public class VoreWorkerDef_Mote : VoreWorkerDef
//    {
//        public FleckDef mote;

//        public override IEnumerable<string> ConfigErrors()
//        {
//            foreach (string error in base.ConfigErrors())
//            {
//                yield return error;
//            }
//            if(mote == null)
//            {
//                yield return "required field \"mote\" not provided";
//            }
//        }
//    }
//}