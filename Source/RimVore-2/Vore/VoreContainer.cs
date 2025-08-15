using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld.Planet;

// base game Pawn_CarryTracker implements carried things similarly, adapted from there

namespace RimVore2
{
    public class VoreContainer : IThingHolder, IExposable
    {
        public IThingHolder ParentHolder => predator;

        private Pawn predator;
        private VorePathDef path;
        private Pawn prey;
        private ThingOwner<Thing> thingOwner;

        public List<Thing> ThingsInContainer => thingOwner.ToList();
        public IEnumerable<Thing> ThingsInContainerAndProductContainer
        {
            get
            {
                foreach(Thing thing in ThingsInContainer)
                    yield return thing;
                if(VoreProductContainer != null)
                    foreach(Thing thing in VoreProductContainer.GetDirectlyHeldThings())
                        yield return thing;
            }
        }
        //public VoreProductContainer VoreProductContainer => (VoreProductContainer)innerContainer.InnerListForReading.Find(thing => thing is VoreProductContainer);
        private bool needToGenerateProductContainer = true;
        private VoreProductContainer voreProductContainer;
        public VoreProductContainer VoreProductContainer
        {
            get
            {
                if(needToGenerateProductContainer)
                {
                    if(RV2Log.ShouldLog(true, "VoreContainer"))
                        RV2Log.Message("Generating product container", false, "VoreContainer");
                    voreProductContainer = path.voreProduct?.CreateProductContainer(path, predator, prey);
                    if(voreProductContainer != null)
                    {
                        // transfer all items that have been "loose" into the created container
                        thingOwner.TryTransferAllToContainer(voreProductContainer.GetDirectlyHeldThings());
                        // then add self to "loose" things
                        TryAddOrTransfer(voreProductContainer);
                        // and then pull the living pawn out of the container again
                        TryAddOrTransfer(prey, false);
                    }
                    needToGenerateProductContainer = false;
                }
                return voreProductContainer;
            }
        }

        public float TotalMass => ThingsInContainerAndProductContainer.Sum(t => t.GetStatValue(StatDefOf.Mass));

        public VoreContainer() { }

        public VoreContainer(Pawn predator, VorePathDef path, Pawn prey = null)
        {
            this.predator = predator;
            thingOwner = new ThingOwner<Thing>(this);
            this.path = path;
            this.prey = prey;
            // creating at construct time means we create the container in the mouth stage, bad use case - create VPC on demand
            //VoreProductContainer = path.voreProduct?.CreateProductContainer(path, predator, prey);
            //TryAddOrTransfer(VoreProductContainer, false);
        }

        public ThingOwner GetDirectlyHeldThings() => thingOwner;

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public void FinishDigestion(Pawn prey, DamageDef appliedDamageDef)
        {
            MovePreyAndInventoryToProductContainer(prey);
            IngestAllIngestibleItems();
            QuirkManager predatorQuirks = predator.QuirkManager();
            if(predatorQuirks != null && predatorQuirks.HasSpecialFlag("DestroyEverything"))
            {
                DestroyAllItems();
            }
            else
            {
                DestroyAllRottableItems();
                DigestAllEquipment(appliedDamageDef);
                DigestAllImplants(prey, appliedDamageDef);
            }
        }


        private void IngestAllIngestibleItems()
        {
            IEnumerable<Thing> ingestibleThings = ThingsInContainerAndProductContainer
                .Where(thing => thing.def.IsIngestible);
            foreach(Thing thing in ingestibleThings.ToList())
            {
                if(!TryRemove(thing))
                {
                    RV2Log.Warning($"Tried to ingest ingestible item {thing}, but it could not be removed from the container!", "VoreContainer");
                    continue;
                }
                thing.Ingested(predator, 0);
                if(RV2Log.ShouldLog(false, "VoreContainer"))
                    RV2Log.Message($"Ingestible item {thing.Label} has been ingested", "VoreContainer");
            }
        }

        /// <summary>
        /// Destroy absolutely everything, except for the prey
        /// </summary>
        private void DestroyAllItems()
        {
            IEnumerable<Thing> allThings = ThingsInContainerAndProductContainer
                .Where(thing => 
                    thing != prey
                    && !thing.def.HasModExtension<ThingDef_VoreProductFlag>()  // do not remove items produced by vore
                    && thing != VoreProductContainer
                );
            foreach(Thing thing in allThings)
            {
                if(!TryRemove(thing))
                {
                    RV2Log.Warning($"Tried to destroy item {thing}, but it could not be removed from the container!", "VoreContainer");
                    continue;
                }
                thing.Destroy();
                if(RV2Log.ShouldLog(false, "VoreContainer"))
                    RV2Log.Message($"Destroyed thing {thing}", "VoreContainer");
            }
        }

        private void DestroyAllRottableItems()
        {
            IEnumerable<Thing> rottableThings = ThingsInContainerAndProductContainer
                .Where(thing => thing.TryGetComp<CompRottable>() != null
                    && !(thing is Pawn || thing is Corpse));    // the pawn is either decomposed or destroyed later, destroying it now would cause the later part of the code to break
            foreach(Thing thing in rottableThings.ToList())
            {
                if(!TryRemove(thing))
                {
                    RV2Log.Warning($"Tried to destroy rottable item {thing}, but it could not be removed from the container!", "VoreContainer");
                    continue;
                }
                thing.Destroy();
                if(RV2Log.ShouldLog(false, "VoreContainer"))
                    RV2Log.Message($"Rottable item {thing.Label} has been digested", "VoreContainer");
            }
        }

        /// <summary>
        /// Digest apparel and equipment(weapons)
        /// </summary>
        private void DigestAllEquipment(DamageDef damageDef)
        {
            IEnumerable<Thing> allEquipment = ThingsInContainerAndProductContainer
                .Where(thing => thing is Apparel || thing.def.IsWeapon);
            if(allEquipment.EnumerableNullOrEmpty())
            {
                return;
            }
            foreach(Thing thing in allEquipment.ToList())
            {
                QuirkManager predatorQuirks = predator.QuirkManager();
                if(predatorQuirks != null)
                {
                    if(predatorQuirks.HasSpecialFlag("DestroyEquipment"))
                    {
                        if(RV2Log.ShouldLog(true, "VoreContainer"))
                            RV2Log.Message($"Predator always destroys equipment, destroying {thing.LabelShort}", false, "VoreContainer");
                        thing.Destroy();
                        TryRemove(thing);
                        continue;
                    }
                }

                int quality = thing.GetQualityOrFallback();
                SimpleCurve qualityDamageCurve = new SimpleCurve()
                {
                    Points =
                    {
                        new CurvePoint(0, 0.9f),
                        new CurvePoint(1, 0.75f),
                        new CurvePoint(2, 0.5f),
                        new CurvePoint(3, 0.45f),
                        new CurvePoint(4, 0.4f),
                        new CurvePoint(5, 0.3f),
                        new CurvePoint(6, 0.15f),
                        new CurvePoint(7, 0.05f)    // this quality doesn't exist, but defines the max value in case mods add more
                    }
                };
                float baseDamage = qualityDamageCurve.Evaluate(quality);
                if(RV2Log.ShouldLog(true, "VoreContainer"))
                    RV2Log.Message($"Calculated equipment damage: {baseDamage}", false, "VoreContainer");
                if(predatorQuirks != null)
                {
                    baseDamage = predatorQuirks.ModifyValue("EquipmentDigestionStrength", baseDamage);
                }
                float equipmentDamage = baseDamage * thing.HitPoints;
                DamageInfo damage = new DamageInfo(damageDef, equipmentDamage, float.MaxValue, -1, predator);
                thing.TakeDamage(damage);

                if(thing is Apparel apparel)
                {
                    bool taintApparel = predatorQuirks?.HasSpecialFlag("NoTaintedApparel") == true ? false : true;

                    if(taintApparel)
                    {
                        // sets the "tainted" status of the apparel (even though the pawn is not dead at this point in time)
                        apparel.Notify_PawnKilled();
                    }
                }
            }
        }

        private void DigestAllImplants(Pawn pawn, DamageDef damageDef)
        {
            float damage = RV2Mod.Settings.cheats.AcidImplantDamage;
            bool destroyImplants = Rand.Chance(RV2Mod.Settings.cheats.ImplantDestructionChance);
            QuirkManager predatorQuirks = predator.QuirkManager();
            if(predatorQuirks != null)
            {
                damage = predatorQuirks.ModifyValue("ImplantDigestionStrength", damage);
                if(predatorQuirks.HasSpecialFlag("DestroyImplants"))
                {
                    destroyImplants = true;
                }
                else if(predatorQuirks.HasSpecialFlag("NeverDestroyImplants"))
                {
                    destroyImplants = false;
                }
            }

            IEnumerable<Hediff_Implant> implants = pawn.health?.hediffSet?.hediffs
                .Where(hediff => hediff is Hediff_Implant)
                .Cast<Hediff_Implant>();
            if(implants.EnumerableNullOrEmpty())
            {
                return;
            }
            foreach(Hediff_Implant implant in implants.ToList())
            {
                if(RV2Log.ShouldLog(true, "VoreContainer"))
                    RV2Log.Message($"Digesting implant {implant.Label}", false, "VoreContainer");
                ThingDef thingDef = destroyImplants ? RV2_Common.DestroyedImplant : implant.def.spawnThingOnRemoved;
                if(thingDef != null)
                {
                    Thing thing = ThingMaker.MakeThing(thingDef);
                    float implantDamage = damage * thing.HitPoints;
                    DamageInfo dinfo = new DamageInfo(damageDef, implantDamage, float.MaxValue, -1, predator);
                    thing.TakeDamage(dinfo);
                    thingOwner.TryAdd(thing);
                    if(RV2Log.ShouldLog(true, "VoreContainer"))
                        RV2Log.Message($"Made implant-item {thing.LabelShort}", false, "VoreContainer");
                }
                pawn.health.RemoveHediff(implant);
            }
        }

        /// <summary>
        /// Move all items from the pawn to container, then move pawn itself to container
        /// </summary>
        private void MovePreyAndInventoryToProductContainer(Pawn prey)
        {
            this.prey = prey;
            // if product container exists, move to that
            if(prey.apparel != null)
            {
                TryAddOrTransfer(prey.apparel.GetDirectlyHeldThings());
            }
            if(prey.inventory != null)
            {
                TryAddOrTransfer(prey.inventory.GetDirectlyHeldThings());
            }
            if(prey.equipment != null)
            {
                TryAddOrTransfer(prey.equipment.GetDirectlyHeldThings());
            }
            TryAddOrTransfer(prey);
        }

        private bool TryRemove(Thing thing)
        {
            if(thingOwner.Contains(thing))
            {
                return thingOwner.Remove(thing);
            }
            ThingOwner productContainerThingOwner = VoreProductContainer.GetDirectlyHeldThings();
            if(productContainerThingOwner.Contains(thing))
            {
                return productContainerThingOwner.Remove(thing);
            }
            if(RV2Log.ShouldLog(false, "VoreContainer"))
                RV2Log.Warning($"Could not remove thing {thing} because it wasn't found in either container", "VoreContainer");
            return false;
        }

        public bool TryAddOrTransferPrey(Pawn prey)
        {
            prey.holdingOwner?.Remove(prey);
            if(TryAddOrTransfer(prey, false))
            {
                if(prey?.Spawned == true)
                {
                    prey.DeSpawn();
                    if(RV2Log.ShouldLog(true, "VoreContainer"))
                        RV2Log.Message("Despawned pawn", false, "VoreContainer");
                }
                return true;
            }
            return false;

        }

        public bool TryAddOrTransfer(IEnumerable<Thing> things, bool addToProductContainerIfAvailable = true)
        {
            if(things.EnumerableNullOrEmpty())
            {
                return true;
            }
            return things
                .ToList()   // transform to list to prevent CollectionModifiedException
                .All(thing => TryAddOrTransfer(thing, addToProductContainerIfAvailable));
        }

        public bool TryAddOrTransfer(ThingOwner oldOwner, bool addToProductContainerIfAvailable = true)
        {
            return oldOwner.All(thing => TryAddOrTransfer(thing));
        }

        public bool TryAddOrTransfer(Thing thing, bool addToProductContainerIfAvailable = true)
        {
            if(thing == null)
            {
                RV2Log.Warning("TryAddOrTransfer called with NULL thing, this should be safely ignorable");
                return true;
            }
            ThingOwner targetOwner;
            // do not retrieve the product container if we are currently generating it, otherwise endless loop
            ThingOwner productOwner = needToGenerateProductContainer ? null : VoreProductContainer?.GetDirectlyHeldThings();
            if(addToProductContainerIfAvailable && productOwner != null)
            {
                targetOwner = productOwner;
            }
            else
            {
                targetOwner = GetDirectlyHeldThings();
            }
            return targetOwner.TryAddOrTransfer(thing);
        }

        public bool TryDropAllThings(bool simulateNullMap = false)
        {
            // if predator is also a prey of another predator, pass to that predator
            if(GlobalVoreTrackerUtility.GetVoreRecord(predator) != null)
            {
                return TryDropForCascadingPredator();
            }
            // if predator is in caravan, pass to caravan inventory
            if(CaravanUtility.IsCaravanMember(predator))
            {
                return TryPassToCaravan(predator);
            }
            // normal drop behaviour on predator map
            return TryDropWithMap(simulateNullMap);
        }

        private bool TryDropForCascadingPredator()
        {
            if(RV2Log.ShouldLog(false, "VoreContainer"))
                RV2Log.Message("Predator is trying to remove all items, but is currently inside of another predator, passing to upper predator", "VoreContainer");
            
            PassToUpperPredator();
            return thingOwner.NullOrEmpty();
        }

        private void PassToUpperPredator()
        {
            VoreTrackerRecord record = predator.GetVoreRecord();
            if(record == null)
            {
                Log.Error($"Could not retrieve vore tracker record for {predator.ToStringSafe()}");
                return;
            }

            // take all pawns directly inside this container and pass to predator
            IEnumerable<Pawn> innerPawns = thingOwner.InnerListForReading
                .Where(t => t is Pawn)
                .Cast<Pawn>();
            foreach(Pawn pawn in innerPawns.ToList())
            {
                record.VoreTracker.SplitOffNewVore(record, pawn, null, record.VorePathIndex - 1);   // -1 to offset to "warmup" that most digest stages use
            }

            record.VoreContainer.TryAddOrTransfer(ThingsInContainerAndProductContainer    // add all items in this container and product container
                .Where(t => !(t is VoreProductContainer))); // but remove the VoreProductContainer so we don't double-wrap
            if(voreProductContainer != null)
            {
                if(VoreProductContainer.GetDirectlyHeldThings().Count == 0)
                {
                    VoreProductContainer.Destroy(); // then delete the old product container
                }
                else
                {
                    RV2Log.Error("VoreProductContainer was not empty when trying to delete it after passing its contents to an upper predator", "VoreContainer");
                }
            }
        }

        private bool TryPassToCaravan(Pawn predator)
        {
            Caravan caravan = CaravanUtility.GetCaravan(this.predator);
            if(caravan == null)
            {
                RV2Log.Warning("Pawn " + predator.LabelShort + " should have been in a caravan");
                return false;
            }
            foreach(Thing item in ThingsInContainer)
            {
                thingOwner.Remove(item);
                if(item is Pawn pawn)
                {
                    Faction playerFaction = Find.FactionManager.OfPlayer;
                    bool forcePlayerFaction = pawn.Faction == playerFaction;
                    Find.WorldPawns.PassToWorld(pawn);
                    if(playerFaction != null && forcePlayerFaction)
                    {
                        pawn.SetFactionDirect(playerFaction);
                    }
                }
                caravan.AddPawnOrItem(item, false);
            }
            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name="predator"></param>
        /// <param name="simulateNullMap">null map can be simulated, this is usually done when kidnapping pawns. Due to the way raider jobs work, the vore can't be ended off-map, it must be ended while the kidnapping predator is still on the map</param>
        /// <returns></returns>
        private bool TryDropWithMap(bool simulateNullMap = false)
        {
            bool success = true;
            Map map = predator.MapHeld;
            bool isMapNull = map == null
                || simulateNullMap;
            bool canPassToOtherThing = CanPassToOtherThing(out Thing containerThing, out ThingOwner thingOwner);
            foreach(Thing thing in ThingsInContainer)
            {
                if(isMapNull)
                {
                    success &= TryDropToNullMap(thing);
                }
                else
                {
                    if(canPassToOtherThing && thingOwner.GetCountCanAccept(thing) > 0)
                    {
                        // if the current thing is a product container and the user wants to "unwrap" the container into the urn
                        if(thing is VoreProductContainer vpc && RV2Mod.Settings.fineTuning.UnwrapOnDisposalContainerRelease)
                        {
                            // transfer the contents of the product container to the new thingOwner
                            vpc.GetDirectlyHeldThings().TryTransferAllToContainer(thingOwner);
                            // and destroy the container
                            vpc.Destroy();
                        }
                        else
                        {
                            success &= thingOwner.TryAddOrTransfer(thing);
                        }
                    }
                    else
                    {
                        success &= TryDropToMap(thing, predator.Position, map);
                    }
                }
            }
            if(canPassToOtherThing && containerThing is Building_DisposalUrn urn)
            {
                urn.RecacheGraphic();
            }
            return success;
        }

        private bool TryDropToMap(Thing thing, IntVec3 position, Map map)
        {
            bool success = this.thingOwner.TryDrop(thing, position, map, ThingPlaceMode.Near, out _);

            if(!success)
            {
                return false;
            }

            if(RV2Mod.Settings.fineTuning.ForbidNonFactionVoreContainer && predator.Faction != Faction.OfPlayer && thing.CanForbid())
            {
                thing.SetForbidden(true);
                return success;
            }
            if(RV2Mod.Settings.fineTuning.AutoHaulVoreContainer && thing.def.designateHaulable)
            {
                thing.Map.designationManager.AddDesignation(new Designation(thing, DesignationDefOf.Haul));
            }
            if(RV2Mod.Settings.fineTuning.AutoOpenVoreContainer && thing is IOpenable openableThing && openableThing.CanOpen)
            {
                thing.Map.designationManager.AddDesignation(new Designation(thing, DesignationDefOf.Open));
            }
            return success;
        }

        private bool CanPassToOtherThing(out Thing containerThing, out ThingOwner thingOwner)
        {
            bool ValidHolder(Thing t)
            {
                if(!(t is IThingHolder th))
                {
                    return false;
                }
                // only accept empty containers
                if(th.GetDirectlyHeldThings().Count > 0)
                {
                    return false;
                }
                return true;
            }

            thingOwner = null;
            containerThing = null;

            if(predator.Map == null)
            {
                return false;
            }
            containerThing = predator.Position.GetThingList(predator.Map)
                .Where(t => t is Building)  // for now restrict passing vore products to buildings, otherwise Pawn thingHolder may interfere
                .FirstOrDefault(t => ValidHolder(t));
            if(containerThing == null)
            {
                return false;
            }
            thingOwner = ((IThingHolder)containerThing).GetDirectlyHeldThings();

            return thingOwner != null;
        }

        private bool TryDropToNullMap(Thing thing)
        {
            if(thing is Pawn pawn)
            {
                // what exactly the world pawns are I truly do not know, but that's where pawns without a map are supposed to go
                if(!Find.WorldPawns.Contains(pawn))
                {
                    Find.WorldPawns.PassToWorld(pawn);
                    return true;
                }
            }
            else
            {
                // nowhere to go for items without a map, so destroy them
                thing.Destroy();
                return true;
            }
            return false;
        }

        public void Tick()
        {
            thingOwner.ThingOwnerTick();
        }

        public void TickRare()
        {
            thingOwner.ThingOwnerTickRare();
        }

        public override string ToString()
        {
            return string.Join(", ", this.thingOwner.InnerListForReading.ConvertAll(thing => thing.ToString()));
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref thingOwner, "innerContainer", new object[]
            {
                this
            });
            Scribe_Values.Look(ref needToGenerateProductContainer, "needToGenerateProductContainer");
            Scribe_References.Look(ref voreProductContainer, "VoreProductContainer");
            Scribe_References.Look(ref predator, "owningPredator");
            Scribe_References.Look(ref prey, "prey");
            Scribe_Defs.Look(ref path, "path");
        }
    }
}