//using RimWorld;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Verse;

//namespace RimVore2
//{
//    public class QuirkHelper_Temporary : QuirkHelper
//    {
//        [TweakValue("RimVore-2", 1, 14400)]  // (ingame time) min 3 minutes, max one year
//        public static int defaultTempQuirkRareTicks = 240;    // (ingame time) one day

//        public QuirkHelper_Temporary(Pawn pawn) : base(pawn) { }

//        public QuirkHelper_Temporary() : base() { }

//        public IEnumerable<TempVoreQuirk> TempQuirks
//        {
//            get
//            {
//                if(quirks.NullOrEmpty())
//                {
//                    return new List<TempVoreQuirk>();
//                }
//                else
//                {
//                    return quirks.Cast<TempVoreQuirk>();
//                }
//            }
//        }

//        public override List<Quirk> Quirks()
//        {
//            return new List<Quirk>();
//        }

//        public override void AddQuirk(Quirk quirk)
//        {
//            AddQuirk(quirk.def, defaultTempQuirkRareTicks);
//        }
//        public override void AddQuirk(QuirkDef quirkDef)
//        {
//            AddQuirk(quirkDef, defaultTempQuirkRareTicks);
//        }

//        public void AddQuirk(QuirkDef quirkDef, int duration)
//        {
//            if(HasQuirk(quirkDef))
//            {
//                RV2Log.Warning("Tried to add temp quirk " + quirkDef.label + " but pawn " + pawn.LabelShort + " already has it.", "Quirks");
//                return;
//            }
//            TempVoreQuirk tempQuirk = new TempVoreQuirk(quirkDef, duration);
//            quirks.Add(tempQuirk);
//            ResolveConflicts();
//            SetStale();
//        }

//        public void AddQuirk(Quirk quirk, int duration)
//        {
//            AddQuirk(quirk.def, duration);
//        }

//        public void AddQuirk(QuirkWithDuration quirkWithDuration)
//        {
//            AddQuirk(quirkWithDuration.quirk, quirkWithDuration.duration);
//        }

//        private void ResolveConflicts()
//        {
//            List<TempVoreQuirk> resolvedQuirks = new List<TempVoreQuirk>();
//            List<QuirkPoolDef> resolvedPools = new List<QuirkPoolDef>();
//            foreach(TempVoreQuirk tempQuirk in TempQuirks)
//            {
//                QuirkPoolDef pool = tempQuirk.Pool;
//                if(pool.poolType == QuirkPoolType.RollForEach)
//                {
//                    resolvedQuirks.Add(tempQuirk);
//                    continue;
//                }
//                // first of its pool, add it
//                if(!resolvedPools.Contains(pool))
//                {
//                    resolvedQuirks.Add(tempQuirk);
//                    resolvedPools.Add(pool);
//                    continue;
//                }
//                // conflict, retrieve the currently dominant quirk, compare its duration with current quirk and determine new dominant quirk
//                TempVoreQuirk oldQuirk = resolvedQuirks.Find(quirk => quirk.Pool == pool);
//                if(tempQuirk.durationLeft > oldQuirk.durationLeft)
//                {
//                    // new dominant quirk, remove the old one
//                    resolvedQuirks.Replace(oldQuirk, tempQuirk);
//                }
//            }
//            quirks.Clear();
//            quirks.AddRange(resolvedQuirks);
//        }

//        public override void CalculateQuirks()
//        {
//            // I have chosen to disable temporary quirks for now, may come back to them in the future
//            return;
//            /*if(quirks == null)
//            {
//                quirks = new List<VoreQuirk>();
//            }
//            else
//            {
//                quirks.Clear();
//            }
//            if(Faction.OfPlayer != null)
//            {
//                CalculateForFactionRelation();
//            }
//            SetStale();*/
//        }

//        public override void Tick()
//        {
//            foreach(TempVoreQuirk quirk in TempQuirks)
//            {
//                quirk.durationLeft--;
//                RV2Log.Message("temp quirk ticked, duration left: " + quirk.durationLeft, true, false, "Quirks");
//                if(quirk.durationLeft == 0)
//                {
//                    RemoveQuirk(quirk);
//                }
//            }
//        }

//        private void CalculateForFactionRelation()
//        {
//            if(pawn.HostileTo(Faction.OfPlayer))
//            {
//                string keyword = "HostileToPlayer";
//                List<TemporaryQuirkGiver> quirkGivers = RV2_Common.TemporaryQuirkGiversForKeyword(keyword);
//                if(quirkGivers.NullOrEmpty())
//                {
//                    RV2Log.Message("Tried to create temporary quirks for pawn " + pawn.LabelShort + ", but found no quirk givers with keyword " + keyword, true, false, "Quirks");
//                    return;
//                }
//                RV2Log.Message("quirk givers: " + string.Join(", ", quirkGivers.ConvertAll(g => g.defName)), true, false, "Quirks");
//                List<QuirkWithDuration> quirksToAdd = quirkGivers    // take all givers
//                    .SelectMany(giver => giver.quirksToApply)   // merge all sub-lists
//                    .ToList();
//                if(quirksToAdd.NullOrEmpty())
//                {
//                    RV2Log.Message("TemporaryQuirkGivers is empty, skipping", "Quirks");
//                    return;
//                }
//                foreach(QuirkWithDuration quirkWithDuration in quirksToAdd)
//                {
//                    AddQuirk(quirkWithDuration);
//                }
//            }
//        }
//    }
//}