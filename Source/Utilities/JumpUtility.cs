using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class VoreJump
    {
        public VoreJump(VorePathDef path, VoreStageDef stage)
        {
            this.path = path;
            this.index = path.stages.IndexOf(stage);
        }

        public VoreJump(VorePathDef path, int index)
        {
            this.path = path;
            this.index = index;
        }

        public VorePathDef path;
        public int index;

        public void Jump(VoreTrackerRecord originalRecord, bool isPathSwitch = false)
        {
            DamageDef damageDef = AcidUtility.GetDigestDamageDef(originalRecord.CurrentVoreStage.def);
            if(damageDef != null)
            {
                DigestionUtility.ApplyDigestionBookmark(originalRecord);
                AcidUtility.ApplyAcidByDigestionProgress(originalRecord, originalRecord.CurrentVoreStage.PercentageProgress, damageDef);
            }
            VoreTracker tracker = originalRecord.VoreTracker;
            tracker.UntrackVore(originalRecord);
            VoreTrackerRecord newRecord = tracker.SplitOffNewVore(originalRecord, originalRecord.Prey, new VorePath(path), index, isPathSwitch);
            DoNotification(originalRecord, newRecord);
        }
        private void DoNotification(VoreTrackerRecord originalRecord, VoreTrackerRecord newRecord)
        {
            string message = "RV2_PredatorSwitchedVoreGoal".Translate(
                newRecord.Predator.Named("PREDATOR"),
                newRecord.Prey.Named("PREY"),
                originalRecord.VoreGoal.LabelCap.Named("PREVIOUSGOAL"),
                newRecord.VoreGoal.LabelCap.Named("NEWGOAL")
            );
            NotificationUtility.DoNotification(RV2Mod.Settings.fineTuning.GoalSwitchNotification, message, "RV2_PredatorSwitchedVoreGoal_Key".Translate());
        }
    }

    public static class JumpUtility
    {
        public static Dictionary<string, IEnumerable<VoreJump>> cachedJumps = new Dictionary<string, IEnumerable<VoreJump>>();
        public static IEnumerable<VoreJump> Jumps(string jumpKey)
        {
            if(!cachedJumps.ContainsKey(jumpKey))
            {
                cachedJumps.Add(jumpKey, GetJumps(jumpKey));
            }
            return cachedJumps[jumpKey];
        }

        private static IEnumerable<VoreJump> GetJumps(string jumpKey)
        {
            foreach(VorePathDef path in DefDatabase<VorePathDef>.AllDefsListForReading)
            {
                VoreStageDef targetStage = path.stages.Find(stage => stage.jumpKey == jumpKey);
                if(targetStage == null) // path does not contain jumpKey
                {
                    continue;
                }
                yield return new VoreJump(path, path.stages.IndexOf(targetStage));
            }
        }

        public static IEnumerable<VoreJump> Jumps(Pawn predator, Pawn prey, string jumpKey)
        {
            return Jumps(jumpKey)
                .Where(jump => jump.path.IsValid(predator, prey, out _));
        }

        public static bool HasJumpKey(VorePathDef path, string jumpKey)
        {
            return path.stages.Any(stage => stage.jumpKey == jumpKey);
        }

        public static IEnumerable<string> JumpKeysFor(VorePathDef path)
        {
            return path.stages
                .Select(stage => stage.jumpKey)
                .Where(key => key != null);
        }
    }
}
