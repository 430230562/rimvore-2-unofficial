using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimVore2
{
    public class VoreRoleHelper
    {
        Pawn initiator;
        Pawn target;
        List<VoreRole> roleWhitelist;
        List<VoreRole> roleBlacklist;
        public VoreRoleHelper(VoreInteractionRequest request)
        {
            initiator = request.Initiator;
            target = request.Target;
            roleWhitelist = request.RoleWhitelist;
            roleBlacklist = request.RoleBlacklist;
        }

        public bool TryRollForRole(out VoreRole role)
        {
            role = VoreRole.Invalid;
            List<VoreRole> validRoles = new List<VoreRole>()
            {
                VoreRole.Predator,
                VoreRole.Prey
                //,VoreRole.Feeder
            };

            RemoveRolesByWhiteBlacklist(ref validRoles);
            RemoveRolesByValidatorChecks(ref validRoles);
            RemoveRolesByInvalidInteractions(ref validRoles);
            RemoveRolesByInvalidPaths(ref validRoles);

            if(!TryMakeWeightedPreference(initiator, validRoles, out Dictionary<VoreRole, float> weightedRoles))
            {
                return false;
            }

            if(RV2Log.ShouldLog(true, "Preferences"))
                RV2Log.Message($"Weighted roles: {LogUtility.ToString(weightedRoles)}", false, "Preferences");
            role = weightedRoles.RandomElementByWeight(weightedRole => weightedRole.Value).Key;
            if(RV2Log.ShouldLog(true, "Preferences"))
                RV2Log.Message($"Picked {role}", false, "Preferences");
            return true;
        }

        public bool TryRollForAbstractRole(out VoreRole role)
        {
            role = VoreRole.Invalid;
            List<VoreRole> validRoles = new List<VoreRole>()
            {
                VoreRole.Predator,
                VoreRole.Prey,
                VoreRole.Feeder
            };
            if(!TryMakeWeightedPreference(initiator, validRoles, out Dictionary<VoreRole, float> weightedRoles))
            {
                return false;
            }
            role = weightedRoles.RandomElementByWeight(weightedRole => weightedRole.Value).Key;
            return true;
        }

        private void RemoveRolesByWhiteBlacklist(ref List<VoreRole> roles)
        {
            if(!roleWhitelist.NullOrEmpty())
            {
                roles.RemoveAll(role => !roleWhitelist.Contains(role));
            }
            if(!roleBlacklist.NullOrEmpty())
            {
                roles.RemoveAll(role => roleBlacklist.Contains(role));
            }
        }

        private void RemoveRolesByValidatorChecks(ref List<VoreRole> roles)
        {
            // check ability to be predator / prey
            bool initiatorCanBePredator = initiator.CanBePredator(out _);
            bool initiatorCanBePrey = initiator.CanBePrey(out _);
            bool targetCanBePredator = target.CanBePredator(out _);
            bool targetCanBePrey = target.CanBePrey(out _);
            if(!initiatorCanBePredator || !targetCanBePrey)
            {
                if(RV2Log.ShouldLog(true, "Preferences"))
                    RV2Log.Message($"Removing predator: initiator can't be predator ? {!initiatorCanBePredator} target can't be prey ? {!targetCanBePrey}", false, "Preferences");
                roles.Remove(VoreRole.Predator);
            }
            if(!initiatorCanBePrey || !targetCanBePredator)
            {
                if(RV2Log.ShouldLog(true, "Preferences"))
                    RV2Log.Message($"Removing prey: initiator can't be prey ? {!initiatorCanBePrey} target can't be predator ? {!targetCanBePredator}", false, "Preferences");
                roles.Remove(VoreRole.Prey);
            }

            // check unwillingness to be predator / prey
            bool initiatorNeverWantsToBePredator = initiator.NeverWantsToBePredator(out _);
            bool initiatorNeverWantsToBePrey = initiator.NeverWantsToBePrey(out _);
            bool targetNeverWantsToBePredator = target.NeverWantsToBePredator(out _);
            bool targetNeverWantsToBePrey = target.NeverWantsToBePrey(out _);

            if(initiatorNeverWantsToBePredator || targetNeverWantsToBePrey)
            {
                if(RV2Log.ShouldLog(true, "Preferences"))
                    RV2Log.Message($"Removing predator: initiator doesn't want to be predator ? {initiatorNeverWantsToBePredator} target doesn't want to be prey ?{targetNeverWantsToBePrey}", false, "Preferences");
                roles.Remove(VoreRole.Predator);
            }
            if(initiatorNeverWantsToBePrey || targetNeverWantsToBePredator)
            {
                if(RV2Log.ShouldLog(true, "Preferences"))
                    RV2Log.Message($"Removing prey: initiator doesn't want to be prey ? {initiatorNeverWantsToBePrey} target doesn't want to be predator ? {targetNeverWantsToBePredator}", false, "Preferences");
                roles.Remove(VoreRole.Prey);
            }
        }
        private void RemoveRolesByInvalidInteractions(ref List<VoreRole> roles)
        {
            bool initiatorAsPredatorIsValid = VoreInteractionManager.Retrieve(new VoreInteractionRequest(initiator, target, VoreRole.Predator)).IsValid;
            bool initiatorAsPreyIsValid = VoreInteractionManager.Retrieve(new VoreInteractionRequest(initiator, target, VoreRole.Prey)).IsValid;
            if(!initiatorAsPredatorIsValid)
            {
                if(RV2Log.ShouldLog(true, "Preferences"))
                    RV2Log.Message($"Removing predator: initiator as predator interaction invalid ? {!initiatorAsPredatorIsValid}", false, "Preferences");
                roles.Remove(VoreRole.Predator);
            }
            if(!initiatorAsPreyIsValid)
            {
                if(RV2Log.ShouldLog(true, "Preferences"))
                    RV2Log.Message($"Removing prey: initiator as prey interaction invalid ? {!initiatorAsPreyIsValid}", false, "Preferences");
                roles.Remove(VoreRole.Prey);
            }
        }
        private void RemoveRolesByInvalidPaths(ref List<VoreRole> roles)
        {
            bool initiatorHasValidPath = !VoreInteractionManager.Retrieve(new VoreInteractionRequest(initiator, target, VoreRole.Predator)).ValidPaths.EnumerableNullOrEmpty();
            bool targetHasValidPath = !VoreInteractionManager.Retrieve(new VoreInteractionRequest(initiator, target, VoreRole.Prey)).ValidPaths.EnumerableNullOrEmpty();
            if(!initiatorHasValidPath)
            {
                if(RV2Log.ShouldLog(true, "Preferences"))
                    RV2Log.Message($"Removing predator: initiator has no valid paths ? {!initiatorHasValidPath}", false, "Preferences");
                roles.Remove(VoreRole.Predator);
            }
            else if(!targetHasValidPath)
            {
                if(RV2Log.ShouldLog(true, "Preferences"))
                    RV2Log.Message($"Removing prey: target has no valid paths ? {!targetHasValidPath}", false, "Preferences");
                roles.Remove(VoreRole.Prey);
            }
        }
        public static bool TryMakeWeightedPreference(Pawn pawn, IEnumerable<VoreRole> roles, out Dictionary<VoreRole, float> weightedPreference)
        {
            weightedPreference = new Dictionary<VoreRole, float>();
            if(roles.EnumerableNullOrEmpty())
            {
                if(RV2Log.ShouldLog(true, "Preferences"))
                    RV2Log.Message("No roles available to make weighted preferences for", false, "Preferences");
                return false;
            }
            foreach(VoreRole role in roles)
            {
                float preference = pawn.PreferenceFor(role);
                if(preference > 0)
                {
                    weightedPreference.Add(role, preference);
                }
                else
                {
                    if(RV2Log.ShouldLog(true, "Preferences"))
                        RV2Log.Message($"Preference for {role} was 0 or lower, not adding", false, "Preferences");
                }
            }
            return !weightedPreference.EnumerableNullOrEmpty();
        }
    }
}
