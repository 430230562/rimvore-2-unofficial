using Verse;
using Verse.AI;

namespace RimVore2
{
    public class VoreJob : Job, IExposable
    {
        public VorePathDef VorePath;
        public VoreProposal Proposal;
        public Pawn Initiator;
        public bool IsForced = false;
        public bool IsKidnapping = false;
        public bool IsRitualRelated = false;

        // we hide base ExposeData here because Tynan thought it was a great idea to completely lock and seal the Job. This entire file is just a mess of workarounds for the restrictive Job class
        public new void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref VorePath, "VorePathDef");
            Scribe_Deep.Look(ref Proposal, "Proposal", new object[0]);
            Scribe_References.Look(ref Initiator, "Initiator");
            Scribe_Values.Look(ref IsKidnapping, "IsKidnapping");
            Scribe_Values.Look(ref IsRitualRelated, "IsRitualRelated");
        }

        public override string ToString()
        {
            return $"{base.ToString()} VorePath: {VorePath?.defName} Initiator: {Initiator?.LabelShort} IsForced: {IsForced} IsKidnapping: {IsKidnapping} IsRitualRelated: {IsRitualRelated}";
        }
    }

    public static class VoreJobMaker
    {
        public static VoreJob MakeJob(Pawn initiator)
        {
            VoreJob job = SimplePool<VoreJob>.Get();
            job.loadID = Find.UniqueIDsManager.GetNextJobID();
            job.Initiator = initiator;
            return job;
        }
        public static VoreJob MakeJob(JobDef jobDef, Pawn initiator)
        {
            VoreJob job = MakeJob(initiator);
            job.def = jobDef;
            return job;
        }
        public static VoreJob MakeJob(JobDef jobDef, Pawn initiator, LocalTargetInfo targetA)
        {
            VoreJob job = MakeJob(initiator);
            job.def = jobDef;
            job.targetA = targetA;
            return job;
        }
        public static VoreJob MakeJob(JobDef jobDef, Pawn initiator, LocalTargetInfo targetA, LocalTargetInfo targetB)
        {
            VoreJob job = MakeJob(initiator);
            job.def = jobDef;
            job.targetA = targetA;
            job.targetB = targetB;
            return job;
        }
        public static VoreJob MakeJob(JobDef jobDef, Pawn initiator, LocalTargetInfo targetA, LocalTargetInfo targetB, LocalTargetInfo targetC)
        {
            VoreJob job = MakeJob(initiator);
            job.def = jobDef;
            job.targetA = targetA;
            job.targetB = targetB;
            job.targetC = targetC;
            return job;
        }
    }
}
