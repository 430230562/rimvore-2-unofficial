using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System;

namespace RimVore2
{
    public class VoreProductContainer : ThingWithComps, IOpenable, IThingHolder, IExposable
    {
        private ThingOwner<Thing> innerContainer;

        public VoreProductContainer() { }

        public ThingComp_DeadPreyInformation DeadPreyComp => GetComp<ThingComp_DeadPreyInformation>();

        public void Initialize(Pawn deadPrey)
        {
            innerContainer = new ThingOwner<Thing>(this);
            InitializeDeadPreyInfo(deadPrey);
        }

        private void InitializeDeadPreyInfo(Pawn prey)
        {
            if(DeadPreyComp == null)
            {
                RV2Log.Warning($"No dead prey info comp found on {LabelShort}!");
                return;
            }
            DeadPreyComp.Init(prey);
        }

        public bool CanOpen => true;

        public int OpenTicks => 50;

        public override string Label
        {
            get
            {
                if(DeadPreyComp == null || DeadPreyComp.ExtraLabel == null)
                {
                    return base.Label;
                }
                return base.Label + DeadPreyComp.ExtraLabel;
            }
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public List<ThingDef> ProvideItems()
        {
            VoreContainerExtension voreContainerExtension = def.GetModExtension<VoreContainerExtension>();
            if(voreContainerExtension != null)
            {
                if(!voreContainerExtension.ProvidesForcedResource())
                {
                    if(RV2Log.ShouldLog(false, "OngoingVore"))
                        RV2Log.Message($"VoreContainerExtension exists for {def.defName} but there are no items to provide.", "OngoingVore");
                    return new List<ThingDef>();
                }
                if(RV2Log.ShouldLog(false, "OngoingVore"))
                    RV2Log.Message($"VoreContainerExtension providing additional items: {string.Join(", ", voreContainerExtension.forcedProducedThings.ConvertAll(thing => thing.defName))}", false, "OngoingVore");
                return voreContainerExtension.forcedProducedThings;
            }
            if(RV2Log.ShouldLog(false, "OngoingVore"))
                RV2Log.Message("No VoreContainerExtension, so no additional items provided", "OngoingVore");
            return new List<ThingDef>();
        }

        public bool AddItems(IEnumerable<Thing> items)
        {
            bool TryAddOrTransfer(Thing thing)
            {
                if(thing.holdingOwner == null)
                {
                    return innerContainer.TryAdd(thing);
                }
                return thing.holdingOwner.TryTransferToContainer(thing, innerContainer);
            }

            return items.All(item => TryAddOrTransfer(item));
        }

        public void Open()
        {
            if(innerContainer.TryDropAll(this.Position, this.Map, ThingPlaceMode.Near))
            {
                this.Destroy();
                return;
            }
            Log.Error("Could not drop items from vore product.");
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<ThingOwner<Thing>>(ref this.innerContainer, "innerContainer", new object[]
            {
                this
            });
        }
    }
}
