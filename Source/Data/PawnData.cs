using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class PawnData : IExposable
    {
        public Pawn Pawn = null;
        public string debug_pawnName;
        public VoreTracker VoreTracker;
        private QuirkManager quirkManager;
        public IdeologyPawnData Ideology = new IdeologyPawnData();
        public ProposalPawnData ProposalData = new ProposalPawnData();
        public RV2_Ownership Ownership;
        public bool CanUseGrapple = true;

        public bool HasQuirks => QuirkManager(false) != null;
        public Dictionary<RV2DesignationDef, RV2Designation> Designations;

        public PawnData() 
        { 
            if(Scribe.mode == LoadSaveMode.Inactive)
            {
                Log.Warning($"Called default constructor for type {this.GetType()} outside of scribing, this will definitely cause issues!");
            }
        }

        public PawnData(Pawn pawn)
        {
            this.Pawn = pawn;
            debug_pawnName = pawn.GetDebugName();
            VoreTracker = new VoreTracker(pawn);
            Ownership = new RV2_Ownership(pawn);
            InitializeDesignations();
        }

        public QuirkManager QuirkManager(bool initializeIfNull = true)
        {
            // if quirk manager hasn't been created yet and the pawn qualifies for quirks, create it
            // Log.Message("quirk manager null? " + (quirkManager == null) + " pawn can have quirks? " + pawn.CanHaveQuirks());
            if(quirkManager == null && initializeIfNull)
            {
                if(Pawn.CanHaveQuirks(out _))
                {
                    if(RV2Log.ShouldLog(false, "Quirks"))
                        RV2Log.Message($"Creating quirk manager for {Pawn.Label}", "Quirks");
                    quirkManager = new QuirkManager(Pawn);
                }
            }
            return quirkManager;
        }

        public void InitializeDesignations()
        {
            if(Pawn == null)
            {
                RV2Log.Warning("Tried to initialize designations, but Pawn is NULL");
                return;
            }
            if(Designations == null)
            {
                Designations = new Dictionary<RV2DesignationDef, RV2Designation>();
            }
            foreach(RV2DesignationDef designation in RV2_Common.VoreDesignations)
            {
                Designations.Add(designation, new RV2Designation(Pawn, designation));
            }

            ScribeUtilities.SyncKeys(ref Designations, RV2_Common.VoreDesignations);
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref Pawn, "Pawn", true);
            Scribe_Values.Look(ref debug_pawnName, "debug_pawnName");
            Scribe_Deep.Look(ref VoreTracker, "VoreTracker", new object[0]);
            Scribe_Deep.Look(ref quirkManager, "quirkManager", Pawn);
            Scribe_Deep.Look(ref Ideology, "Ideology");
            Scribe_Deep.Look(ref ProposalData, "ProposalData");
            Scribe_Deep.Look(ref Ownership, "ownership");
            Scribe_Values.Look(ref CanUseGrapple, "CanUseGrapple");
            if(Scribe.mode == LoadSaveMode.Saving)
            {
                ScribeUtilities.SyncKeys(ref Designations, RV2_Common.VoreDesignations);
            }
            ScribeUtilities.ScribeVariableDictionary(ref Designations, "designations", LookMode.Def, LookMode.Deep);
            if(Scribe.mode == LoadSaveMode.LoadingVars)
            {
                ScribeUtilities.SyncKeys(ref Designations, RV2_Common.VoreDesignations);
                if(Ideology == null)
                {
                    Ideology = new IdeologyPawnData();
                }
                if(ProposalData == null)
                {
                    ProposalData = new ProposalPawnData();
                }
                if(debug_pawnName == null)
                {
                    debug_pawnName = Pawn.GetDebugName();
                }
            }
        }

        public bool IsValid
        {
            get
            {
                return Pawn != null 
                    && VoreTracker != null
                    && !Pawn.Discarded;
            }
        }
    }
}
