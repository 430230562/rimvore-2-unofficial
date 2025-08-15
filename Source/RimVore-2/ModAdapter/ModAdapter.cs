using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public static class ModAdapter
    {
        //private static ModAdapter_HAR har;
        private static IGenitalAccess genitals;

        public static bool IsRJWLoaded => ModLister.AnyFromListActive(new List<string>() { "rim.job.world" });

        public static IGenitalAccess Genitals
        {
            get
            {
                if(genitals == null)
                {
#if v1_4
                    string version = "1.4";
#elif v1_5
                    string version = "1.5";
#endif

                    if (IsRJWLoaded)
                    {
                        string filePath = $"{ReflectionUtility.ModDirectory}/MajorModIntegrations/RimJobWorld/{version}/Assemblies/RV2_RJW_Integration.dll";
                        Assembly rjwAssembly = Assembly.LoadFrom(filePath);
                        // Prepatcher loads these assemblies as ReflectionOnly, breaking this method
                        // If the assembly is ReflectionOnly for any reason, reload them into the current app domain
                        if (rjwAssembly.ReflectionOnly)
                        {
                            rjwAssembly = Assembly.Load(rjwAssembly.FullName);
                        }
                        //Log.Message(rjwAssembly.ToString());
                        Type genitalAccessType = rjwAssembly.GetType("RV2_RJW.GenitalAccess");
                        genitals = (IGenitalAccess)Activator.CreateInstance(genitalAccessType);
                    }
                    else
                    {
                        string filePath = $"{ReflectionUtility.ModDirectory}/LightGenitals/{version}/Assemblies/LightGenitals.dll";
                        Assembly lgAssembly = Assembly.LoadFrom(filePath);
                        if (lgAssembly.ReflectionOnly)
                        {
                            lgAssembly = Assembly.Load(lgAssembly.FullName);
                        }

                        Type genitalAccessType = lgAssembly.GetType("LightGenitals.GenitalAccess");
                        genitals = (IGenitalAccess)Activator.CreateInstance(genitalAccessType);
                    }
                }
                return genitals;
            }
        }
    }
}
