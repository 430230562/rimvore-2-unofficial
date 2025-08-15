using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RimVore2
{
    // this is just a horrible way of doing it, but also the easiest, it's just way too complex to create abstract methods for
    public static class ColonyRelationUtility
    {
        public static List<RelationKind> GetRelationKinds(this Pawn pawn)
        {
            List<RelationKind> relations = new List<RelationKind>();
            if(pawn.IsQuestLodger() || pawn.IsQuestHelper())
            {
                relations.Add(RelationKind.TempColonist);
            }
            else if(pawn.IsColonist)
            {
                relations.Add(RelationKind.Colonist);
            }
            if(pawn.GuestStatus == GuestStatus.Guest)
                relations.Add(RelationKind.Guest);

            // need to make sure these are not called for the player faction, because the evaluation of "is hostile" causes issues when trying to evaluate the relation of a faction to itself (thanks tynan)
            if(Faction.OfPlayerSilentFail != null && pawn.Faction != Faction.OfPlayer)
            {
                if(pawn.Faction == null)
                    relations.Add(RelationKind.Factionless);
                else if(pawn.Faction.PlayerRelationKind == FactionRelationKind.Hostile)
                    relations.Add(RelationKind.Raider);
                else if(pawn.Faction.PlayerRelationKind == FactionRelationKind.Neutral || pawn.Faction?.PlayerRelationKind == FactionRelationKind.Ally)
                    relations.Add(RelationKind.Visitor);
            }

            if(pawn.IsPrisonerOfColony)
                relations.Add(RelationKind.Prisoner);
            if(pawn.TraderKind != null)
                relations.Add(RelationKind.Trader);
            if(pawn.IsWildMan())
                relations.Add(RelationKind.WildMan);

            if(pawn.RaceProps?.IsMechanoid == true)
                relations.Add(RelationKind.Mechanoid);
            if(pawn.RaceProps?.Animal == true)
            {
                relations.Add(RelationKind.Animal);
                if(pawn.Faction?.IsPlayer == true)
                    relations.Add(RelationKind.ColonyAnimal);
                else
                {
                    // remove factionless relation, it is redundant when the pawn is a wild animal
                    if(relations.Contains(RelationKind.Factionless))
                        relations.Remove(RelationKind.Factionless);
                    relations.Add(RelationKind.WildAnimal);
                }
            }
            if(pawn.IsSlaveOfColony)
                relations.Add(RelationKind.Slave);
            return relations;
        }

        public static bool TryGetColonyRelations(this Pawn pawn, out List<RelationKind> relations)
        {
            relations = GetRelationKinds(pawn);
            if(relations.NullOrEmpty())
            {
                return false;
            }
            if(RV2Log.ShouldLog(true, "Relations"))
                RV2Log.Message($"Pawn {pawn.LabelShort} has these relations: {string.Join(", ", relations.ConvertAll(relation => relation.ToString()))}", true, "Relations", (pawn + "relations").GetHashCode());
            return true;
        }
    }
}