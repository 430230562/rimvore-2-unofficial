using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public class RV2Designation : IExposable
    {
        public RV2DesignationDef def;
        public Pawn pawn;
        public bool enabledAuto;
        public bool enabledManual;
        public bool isSetManually;

        public RV2Designation()
        {
            if(Scribe.mode == LoadSaveMode.Inactive)
            {
                Log.Warning($"Called default constructor for type {this.GetType()} outside of scribing, this will definitely cause issues!");
            }
        }

        public RV2Designation(Pawn pawn, RV2DesignationDef def)
        {
            this.pawn = pawn;
            this.def = def;
            enabledManual = false;
            isSetManually = false;
            CalculateEnabledAuto();
        }

        public Texture2D CurrentIcon() => def.CurrentIcon(this);
        public string CurrentTip() => def.CurrentTip(this);

        public void CalculateEnabledAuto()
        {
            enabledAuto = RV2Mod.Settings.rules.DesignationActive(pawn, def);
        }

        public bool IsEnabled()
        {
            if(def.IsBlocked(pawn))
            {
                return false;
            }
            if(isSetManually)
            {
                return enabledManual;
            }
            CalculateEnabledAuto();
            return enabledAuto;
        }

        // auto -> manual on -> manual off -> <repeat>
        public void Cycle()
        {
            // can't cycle if designation is blocked
            if(def.IsBlocked(pawn))
            {
                return;
            }
            VoreInteractionManager.Reset(pawn);
            if(isSetManually)
            {
                if(enabledManual)
                {
                    enabledManual = false;
                }
                else
                {
                    isSetManually = false;
                    CalculateEnabledAuto();
                }
            }
            else
            {
                enabledManual = true;
                isSetManually = true;
            }
            VoreInteractionManager.Reset(pawn);
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_References.Look(ref pawn, "pawn", true);
            Scribe_Values.Look(ref enabledAuto, "enabledAuto");
            Scribe_Values.Look(ref enabledManual, "enabledManual");
            Scribe_Values.Look(ref isSetManually, "isSetManually");
        }
    }
}
