using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimVore2
{
    public abstract class SettingsContainer : IExposable
    {
        public virtual bool DevModeOnly => false;
        public SettingsContainer() { }
        public abstract void Reset();
        public abstract void EnsureSmartSettingDefinition();
        public virtual void DefsLoaded()
        {
            EnsureSmartSettingDefinition();
        }
        public virtual void ExposeData()
        {
            EnsureSmartSettingDefinition();
        }

        public virtual void PostExposeData()
        {
            if(Scribe.mode == LoadSaveMode.LoadingVars)
            {
                EnsureSmartSettingDefinition();
            }
        }
    }
}
