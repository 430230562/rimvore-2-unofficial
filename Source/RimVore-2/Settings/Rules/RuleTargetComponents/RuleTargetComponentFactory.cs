using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public static class RuleTargetComponentFactory
    {
        public static List<string> Keys;
        static Dictionary<string, Func<RuleTargetComponent>> createableComponents;

        static RuleTargetComponentFactory()
        {
            InitializeTargetCreator();
        }

        public static RuleTargetComponent CreateInitialComponent()
        {
            return createableComponents.FirstOrDefault().Value();
        }

        public static RuleTargetComponent CreateComponent(string key)
        {
            if(!createableComponents.ContainsKey(key))
                return null;
            return createableComponents[key]();
        }

        /// <summary>
        /// Runs through all non-abstract subclasses of RuleTargetComponent,
        /// creates a temporary instance,
        /// fetches the label from it,
        /// then populates the dictionary with the label as key and the creation through Activator as value
        /// </summary>
        private static void InitializeTargetCreator()
        {
            IEnumerable<Type> possibleTargetTypes = typeof(RuleTargetComponent)
                .AllSubclassesNonAbstract();
            createableComponents = new Dictionary<string, Func<RuleTargetComponent>>();
            foreach(Type type in possibleTargetTypes)
            {
                try
                {
                    Func<RuleTargetComponent> createComponent = () => (RuleTargetComponent)Activator.CreateInstance(type);
                    RuleTargetComponent tempComponent = createComponent();
                    if(tempComponent.RequiresIdeology && !ModsConfig.IdeologyActive)
                        continue;
                    if(tempComponent.RequiresIdeology && !ModsConfig.RoyaltyActive)
                        continue;
                    if(tempComponent.RequiresBiotech && !ModsConfig.BiotechActive)
                        continue;
                    string componentKey = tempComponent.ButtonTranslationKey;
                    // if the dictionary already contains the key (user error when creating labels), use a random number append
                    if(createableComponents.ContainsKey(componentKey))
                        componentKey += $"_{Rand.Int}";
                    createableComponents[componentKey] = createComponent;
                }
                catch(Exception e)
                {
                    Log.Error("Error while trying to generate RuleTarget entries: " + e);
                }
            }
            Keys = createableComponents.Keys.ToList();
        }
    }
}
