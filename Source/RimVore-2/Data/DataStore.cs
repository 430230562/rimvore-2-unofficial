using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld.Planet;

namespace RimVore2
{
    [Obsolete]
    class DataStore : WorldComponent
    {
        public bool migratedToNewPawnData = true;

        public DataStore(World world) : base(world)
        { }

        public Dictionary<int, PawnData> PawnData = new Dictionary<int, PawnData>();

        public override void ExposeData()
        {
            if(Scribe.mode == LoadSaveMode.Saving)
            {
                PawnData.RemoveAll(item => item.Value == null || !item.Value.IsValid);
            }
            base.ExposeData();
            Scribe_Collections.Look(ref PawnData, "Data", LookMode.Value, LookMode.Deep);
            if(Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if(PawnData == null)
                {
                    PawnData = new Dictionary<int, PawnData>();
                }
            }
            Scribe_Values.Look(ref migratedToNewPawnData, nameof(migratedToNewPawnData), false);
            if(Scribe.mode == LoadSaveMode.LoadingVars && !migratedToNewPawnData)
            {
                Log.Message($"Migrating RV2 pawnData to new storage solution, this should not impact your colony at all");
                Migrate();
            }
        }

        public void Migrate()
        {
            RV2Mod.RV2Component.MigrateFromOldPawnData(PawnData);
            PawnData.Clear();
            migratedToNewPawnData = true;
        }
    }
}
