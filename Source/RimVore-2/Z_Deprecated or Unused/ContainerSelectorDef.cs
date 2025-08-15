/*using System;
using System.Collections.Generic;
using Verse;

namespace RimVore2
{
    public class ContainerSelectorDef : Def
    {
        private ThingDef NoneValue => RV2_Common.VoreContainerNone;
        private readonly ThingDef defaultValue = null;
        private readonly ThingDef bonesValue = null;
        private readonly ThingDef scatValue = null;
        private readonly ThingDef scatAndBonesValue = null;
        private readonly ThingDef fallbackValue = null;

        private bool ScatEnabled => RV2Settings_General.ScatEnabled;
        private bool BonesEnabled => RV2Settings_General.BonesEnabled;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string configError in base.ConfigErrors())
            {
                yield return configError;
            }
            if(defaultValue == null)
            {
                yield return "Default value not set";
            }
            if(fallbackValue == null)
            {
                yield return "Fallback value not set";
            }
            if(scatValue == NoneValue || scatValue == "")
            {
                yield return "scatValue is set to \"None\" value or empty, will cause validation errors, remove the node instead";
            }
            if(bonesValue == NoneValue || bonesValue == "")
            {
                yield return "bonesValue is set to \"None\" value or empty, will cause validation errors, remove the node instead";
            }
            if(scatAndBonesValue == NoneValue || scatAndBonesValue == "")
            {
                yield return "scatAndBonesValue is set to \"None\" value or empty, will cause validation errors, remove the node instead";
            }
            if(fallbackValue == scatValue)
            {
                yield return "fallbackValue is set to the same as \"scatValue\" which will cause problems when scat is disabled";
            }
            if(fallbackValue == bonesValue)
            {
                yield return "fallbackValue is set to the same as \"bonesValue\" which will cause problems when bones are disabled";
            }
            if(fallbackValue == scatAndBonesValue)
            {
                yield return "fallbackValue is set to the same as \"scatAndBonesValue\" which will cause problems when scat or bones are disabled";
            }
        }

        public ThingDef GetAllowedContainer()
        {
            ThingDef container = defaultValue;
            // if scat is disabled and the default value has anything to do with scat, replace with bones if bones enabled, otherwise set to fallback
            if(!ScatEnabled && (container == scatValue || container == scatAndBonesValue))
            {
                if(BonesEnabled && bonesValue != null)
                {
                    // Log.Message("scat disabled, bones enabled");
                    container = bonesValue;
                }
                else
                {
                    // Log.Message("scat disabled, bones disabled");
                    container = fallbackValue;
                }
            }
            // if bones are disabled and the default value has anything to do with bones, replace with scat if scat enabled, otherwise set to fallback
            if(!BonesEnabled && (container == bonesValue || container == scatAndBonesValue))
            {
                if(ScatEnabled && scatValue != null)
                {
                    // Log.Message("scat enabled, bones disabled");
                    container = scatValue;
                }
                else
                {
                    // Log.Message("scat disabled, bones disabled");
                    container = fallbackValue;
                }
            }
            return container;
        }

        public List<T> GetValidValues<T>(bool isNoneValid = false) where T : Def
        {
            List<string> validValues = GetValidValues(isNoneValid);
            return validValues.ConvertAll(name => DefDatabase<T>.GetNamed(name));
        }

        public List<string> GetValidValues(bool isNoneValid = false)
        {
            List<string> validValues = new List<string>();
            if(IsValueValid(defaultValue))
            {
                // Log.Message("adding valid default");
                validValues.Add(defaultValue);
            }
            if(IsValueValid(bonesValue))
            {
                // Log.Message("adding valid bones");
                validValues.Add(bonesValue);
            }
            if(IsValueValid(scatValue))
            {
                // Log.Message("adding valid scat");
                validValues.Add(scatValue);
            }
            if(IsValueValid(scatAndBonesValue))
            {
                // Log.Message("adding valid scat and bones");
                validValues.Add(scatAndBonesValue);
            }
            validValues.Add(fallbackValue);
            if(isNoneValid)
            {
                // Log.Message("adding valid none");
                validValues.Add(NoneValue);
            }
            return validValues;
        }

        public bool IsValueValid(ThingDe value)
        {
            if(value == "INVALID")
            {
                return false;
            }
            if(value == null || value == "")
            {
                return false;
            }
            if(!ScatEnabled && (value == scatValue || value == scatAndBonesValue))
            {
                return false;
            }
            if(!BonesEnabled && (value == bonesValue || value == scatAndBonesValue))
            {
                return false;
            }
            return true;
        }

        public T GetSettingsDeterminedDef<T>(string productKey) where T : Def
        {
            string containerKey = GetSettingsDeterminedValue(productKey);
            if(containerKey == null)
            {
                throw new Exception("RV2: container set by settings returned a null key");
            }
            return DefDatabase<T>.GetNamed(containerKey);
        }

        public string GetSettingsDeterminedValue(string productKey)
        {
            string containerValue = RV2Settings_General.VorePathResultingContainer.TryGetValue(productKey, null);
            return containerValue;
        }
    }
}*/