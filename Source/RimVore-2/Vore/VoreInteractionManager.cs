using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    /// <summary>
    /// Uses a LRU (least recently used) cache to keep track of previously calculated interactions
    /// </summary>
    public static class VoreInteractionManager
    {
        public static int CacheLimit => RV2Mod.Settings.debug.MaxCachedInteractions;
        static Queue<VoreInteraction> cachedInteractions = new Queue<VoreInteraction>();
        /// <summary>
        /// Retrieve a cached VoreInteraction or create a new VoreInteraction if there is none cached. When called with InitiatorRole == Invalid, a preferred role will be calculated and an appropriate VoreInteraction will be returned if possible
        /// </summary>
        public static VoreInteraction Retrieve(VoreInteractionRequest request)
        {
            // when we don't have a role determined already, we need to calculate both potential roles and then pick the most preferred one
            if(request.InitiatorRole == VoreRole.Invalid)
                return RetrieveForUnknownRole(request);
            else
                return InternalRetrieve(request);
        }
        /// <summary>
        /// Called internally, can retrieve Interaction for request.InitiatorRole == VoreRole.Invalid
        /// </summary>
        private static VoreInteraction InternalRetrieve(VoreInteractionRequest request)
        {
            VoreInteraction interaction = cachedInteractions.FirstOrDefault(i => i.AppliesTo(request));
            if(interaction != null)
            {
                if(RV2Log.ShouldLog(true, "VoreInteractions"))
                    RV2Log.Message($"Found cached interaction for predator {interaction.Predator?.LabelShort}, prey {interaction.Prey?.LabelShort}", true, "VoreInteractions");
                // move interaction to end of queue to "refresh" its usage and prevent it from being de-queued quickly
                cachedInteractions.Move(cachedInteractions.FirstIndexOf(i => i == interaction), cachedInteractions.Count - 1);
                return interaction;
            }
            // no interaction exists yet, create it and enqueue it
            interaction = new VoreInteraction(request);
            cachedInteractions.Enqueue(interaction);
            if(RV2Log.ShouldLog(true, "VoreInteractions"))
                RV2Log.Message($"Cached new interaction predator {interaction.Predator?.LabelShort}, prey {interaction.Prey?.LabelShort}:\n{interaction}", true, "VoreInteractions");
            if(cachedInteractions.Count > CacheLimit)
            {
                VoreInteraction removedInteraction = cachedInteractions.Dequeue();
                if(RV2Log.ShouldLog(true, "VoreInteractions"))
                    RV2Log.Message($"Cached interactions exceeded caching limit of {CacheLimit}, removing oldest cached interaction: Predator: {removedInteraction.Predator} Prey: {removedInteraction.Prey}", true, "VoreInteractions");
            }
            return interaction;
        }

        private static VoreInteraction RetrieveForUnknownRole(VoreInteractionRequest request)
        {
            if(RV2Log.ShouldLog(false, "VoreInteractions"))
                RV2Log.Message("Trying to retrieve interaction for unknown initiator role. Calculating for both VoreRole.Prey and VoreRole.Predator and picking best fit", "VoreInteractions");
            VoreRoleHelper roleHelper = new VoreRoleHelper(request);
            if(roleHelper.TryRollForRole(out VoreRole preferredRole))
            {
                request.InitiatorRole = preferredRole;
            }
            return InternalRetrieve(request);
        }

        public static void ClearCachedInteractions()
        {
            cachedInteractions.Clear();
            if(RV2Log.ShouldLog(false, "VoreInteractions"))
                RV2Log.Message("Removed all cached interactions", false, "VoreInteractions");
        }

        public static void Reset(Pawn pawn)
        {
            int previousInteractionCount = cachedInteractions.Count;
            // sadly Queue has no RemoveAll implementation, so we just create a new Queue from a filtered down list of still valid interactions
            cachedInteractions = new Queue<VoreInteraction>(
                cachedInteractions.Where(interaction =>
                    interaction.Predator != pawn
                    && interaction.Prey != pawn
                    && interaction.Initiator != pawn)
                );
            if(RV2Log.ShouldLog(true, "VoreInteractions"))
                RV2Log.Message($"Reset cached interactions for {pawn.LabelShort}, cache shrunk from {previousInteractionCount} to {cachedInteractions.Count}.", true, "VoreInteractions");
        }
    }
}
