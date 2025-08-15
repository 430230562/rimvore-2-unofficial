using System;
using System.Collections.Generic;
using Verse;

namespace RimVore2
{
    /// <summary>
    /// Re-pack digested remains and/or add new items
    /// </summary>
    /// <remarks>
    /// Following possibilities: 
    /// * items set, container and containerItems not set: The normal contents of the VoreContainer are excreted alongside any Thing items provided
    /// * items, container set and containerItems not set: The normal contents of the VoreContainer are moved the container item, which is excreted alongside other items (like spawned filth)
    /// * items, container and containerItems set: The normal contents of the VoreContainer are moved to the container item along with whatever items were provided in containerItems. The container alongside the provided items will be excreted
    /// </remarks>
    public class VoreProduct
    {
        public bool dessicatePrey = true;
        public bool destroyPrey = false;

        public List<ThingDef> items;
        public List<ThingDef> containerItems;
        //public ContainerSelectorDef containerSelector;
        public List<ThingDef> selectableContainers;

        public List<ThingDef> ValidContainers => selectableContainers.FindAll(container =>
        {
            var extension = container.GetModExtension<VoreContainerExtension>();
            if(extension == null)
            {
                return false;
            }
            return extension.IsValid();
        });

        public IEnumerable<string> ConfigErrors()
        {
            foreach(ThingDef container in selectableContainers)
            {
                if(!container.HasModExtension<VoreContainerExtension>())
                {
                    yield return $"ENtry in {nameof(selectableContainers)} {container.defName} does not have {nameof(VoreContainerExtension)} DefModExtension! Remove the entry for selectable containers or add the DefModExtension to the ThingDef!";
                }
            }
        }

        public void AddVoreProducts(VoreTrackerRecord record)
        {
            if(record.VoreContainer == null)
            {
                RV2Log.Error("No VoreContainer for record " + record.ToString());
                return;
            }
            VoreProductContainer productContainer = record.VoreContainer.VoreProductContainer;
            if(productContainer != null)
            {
                // move all items in the non-product container into the product container (this moves the corpse into the product container)
                //RV2Log.Message("Transfering all items from VoreContainer to VoreProductContainer", "VoreContainer");
                //record.VoreContainer.GetDirectlyHeldThings().TryTransferAllToContainer(productContainer.GetDirectlyHeldThings());
                // then move the VoreProductContainer INTO the VoreContainer
                //record.VoreContainer.TryAddItem(productContainer);
                // produce additional items for the product container
                if(!containerItems.NullOrEmpty())
                {
                    if(RV2Log.ShouldLog(false, "VoreContainer"))
                        RV2Log.Message("Creating container items", "VoreContainer");
                    List<Thing> producedContainerItems = containerItems?.ConvertAll(thingDef => ThingMaker.MakeThing(thingDef));
                    if(!producedContainerItems.NullOrEmpty())
                    {
                        if(RV2Log.ShouldLog(true, "VoreContainer"))
                            RV2Log.Message($"producedContainerItems: {string.Join(", ", producedContainerItems.ConvertAll(t => t.ToString()))}", false, "VoreContainer");
                        productContainer.GetDirectlyHeldThings().TryAddRangeOrTransfer(producedContainerItems);
                    }
                }
            }
            else
            {
                if(RV2Log.ShouldLog(false, "VoreContainer"))
                    RV2Log.Message("No VoreProductContainer!");
            }
            // produce items that are excreted *alongside* the product container
            if(!items.NullOrEmpty())
            {
                if(RV2Log.ShouldLog(false, "VoreContainer"))
                    RV2Log.Message("Creating items", "VoreContainer");
                List<Thing> producedItems = containerItems?.ConvertAll(thingDef => ThingMaker.MakeThing(thingDef));
                if(!producedItems.NullOrEmpty())
                {
                    if(RV2Log.ShouldLog(true, "VoreContainer"))
                        RV2Log.Message($"producedItems: {string.Join(", ", producedItems.ConvertAll(t => t.ToString()))}", false, "VoreContainer");
                    record.VoreContainer.TryAddOrTransfer(producedItems, false);
                }
            }
        }

        /// If the predator has an override for this path, use that container, otherwise use the path-specific containers
        public VoreProductContainer CreateProductContainer(VorePathDef vorePathDef, Pawn predator, Pawn prey = null)
        {
            ThingDef containerDef;
            RaceDef_AlternativeVoreProductContainer overrideContainerExtension = predator.def.GetModExtension<RaceDef_AlternativeVoreProductContainer>();
            if(overrideContainerExtension != null)
            {
                containerDef = overrideContainerExtension.AlternativeContainerFor(vorePathDef);
            }
            else
            {
                if(ValidContainers.NullOrEmpty())
                {
                    return null;
                }
                containerDef = RV2Mod.Settings.rules.GetContainer(predator, vorePathDef);
            }
            if(containerDef == null)
            {
                return null;
            }
            VoreProductContainer productContainer = (VoreProductContainer)ThingMaker.MakeThing(containerDef);
            productContainer.Initialize(prey);
            return productContainer;
        }
    }
}
