using BaseLib.Utils;
using HarmonyLib;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace KomeijiKoishi.Relics;

[Pool(typeof(KoishiSharedRelicPool))]
public sealed class Dream_Koishi : KoishiCollaborationRelic
{
    private const int FreeTurns = 3;

    public override Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player != Owner || player.Creature.IsDead || CombatManager.Instance.IsOverOrEnding
            || player.PlayerCombatState?.TurnNumber != FreeTurns + 1)
            return Task.CompletedTask;

        Flash();
        return CreatureCmd.Kill(player.Creature, force: true);
    }

    public override bool TryModifyEnergyCostInCombatLate(CardModel card, decimal originalCost, out decimal modifiedCost) =>
        TryMakeFree(card, originalCost, out modifiedCost);

    public override bool TryModifyStarCost(CardModel card, decimal originalCost, out decimal modifiedCost) =>
        TryMakeFree(card, originalCost, out modifiedCost);

    private bool TryMakeFree(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        bool applies = card.Owner == Owner && IsFreeTurn(Owner) && card.Pile?.Type is PileType.Hand or PileType.Play;
        modifiedCost = applies ? 0m : originalCost;
        return applies;
    }

    private static bool IsFreeTurn(Player player) => player.Creature.IsAlive
        && player.PlayerCombatState is { TurnNumber: > 0 and <= FreeTurns };

    private static bool HasFreeDream(CardModel card) =>
        card.Owner is { } owner && IsFreeTurn(owner) && owner.Relics.Any(relic => relic is Dream_Koishi && !relic.IsMelted);

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.SpendResources))]
    private static class FreeResourcesPatch
    {
        private static bool Prefix(CardModel __instance, ref Task<(int, int)> __result)
        {
            if (__instance.CombatState is not { } combat
                || __instance.Owner?.PlayerCombatState is not { } playerState
                || !HasFreeDream(__instance))
                return true;

            __result = CaptureWithoutSpending(__instance, combat, playerState);
            return false;
        }

        private static async Task<(int, int)> CaptureWithoutSpending(
            CardModel card, ICombatState combat, PlayerCombatState playerState)
        {
            if (card.EnergyCost.CostsX)
                card.EnergyCost.CapturedXValue = playerState.Energy;

            card.LastStarsSpent = card.HasStarCostX ? playerState.Stars : 0;
            await Hook.AfterEnergySpent(combat, card, 0);
            return (0, 0);
        }
    }

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
    private static class FreeXValuesPatch
    {
        private static void Prefix(CardModel __instance, bool isAutoPlay, ref ResourceInfo resources)
        {
            if (isAutoPlay || !HasFreeDream(__instance) || (!__instance.EnergyCost.CostsX && !__instance.HasStarCostX))
                return;

            resources = new ResourceInfo
            {
                EnergySpent = 0,
                EnergyValue = __instance.EnergyCost.CostsX ? __instance.EnergyCost.CapturedXValue : 0,
                StarsSpent = 0,
                StarValue = __instance.HasStarCostX ? __instance.LastStarsSpent : 0
            };
        }
    }
}
