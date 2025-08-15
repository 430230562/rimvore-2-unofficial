using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public abstract class QuirkLayer : IExposable, ICloneable
    {
        protected Pawn pawn;
        public bool IsStale = true;
        protected List<Quirk> quirks;

        public QuirkLayer(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public QuirkLayer() { }
        public abstract void CalculateQuirks();

        public virtual List<Quirk> Quirks()
        {
            if(pawn == null)
            {
                Log.Warning($"Tried to retrieve quirks for NULL pawn");
                return new List<Quirk>();
            }
            if(quirks == null)
            {
                CalculateQuirks();
            }
            if(quirks == null)
            {
                RV2Log.Warning("Quirks should have been calculated and cached, but were null. Returning empty list.", false, "Quirks");
                return new List<Quirk>();
            }
            return quirks;
        }

        public virtual void AddQuirk(QuirkDef quirkDef)
        {
            if(quirkDef == null)
            {
                Log.Warning($"Tried to add NULL quirk def to {pawn.ToStringSafe()}'s quirks");
                return;
            }
            if(HasQuirk(quirkDef))
            {
                RV2Log.Warning("Tried to add quirk " + quirkDef?.label + " but pawn " + pawn?.LabelShort + " already has it.", false, "Quirks");
                return;
            }
            quirks.Add(new Quirk(quirkDef));
            SetStale();
        }
        public virtual void AddQuirk(Quirk quirk)
        {
            AddQuirk(quirk.def);
        }

        public virtual void RemoveQuirk(QuirkDef quirkDef)
        {
            if(!HasQuirk(quirkDef))
            {
                RV2Log.Warning("Tried to remove quirk " + quirkDef.label + " but pawn " + pawn.LabelShort + " does not have it.", false, "Quirks");
            }
            quirks = quirks.FindAll(quirk => quirk.def != quirkDef);
            SetStale();
        }

        public virtual void RemoveQuirk(Quirk quirk)
        {
            RemoveQuirk(quirk.def);
        }

        public virtual bool HasQuirk(QuirkDef quirkDef)
        {
            if(quirks.NullOrEmpty())
            {
                return false;
            }
            return quirks.Any(quirk => quirk.def == quirkDef);
        }

        public virtual bool HasAllQuirksInPool(QuirkPoolDef pool)
        {
            if(Quirks().NullOrEmpty())
            {
                return false;
            }
            return pool.quirks.TrueForAll(searchQuirk => Quirks().Any(persistentQuirk => persistentQuirk.def == searchQuirk));
        }
        public void SetStale()
        {
            IsStale = true;
            RV2_Patch_UI_Widget_GetGizmos.NotifyPawnStale(pawn);
        }

        public virtual void Tick() { }

        public virtual void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn", true);
            Scribe_Collections.Look(ref quirks, "quirks", LookMode.Deep);
        }

        public abstract object Clone();

    }
}