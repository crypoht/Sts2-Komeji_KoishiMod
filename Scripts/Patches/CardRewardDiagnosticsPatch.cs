using System;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace KomeijiKoishi.Patches
{
    [HarmonyPatch(typeof(CardReward), nameof(CardReward.Populate))]
    public static class CardRewardDiagnosticsPatch
    {
        private static readonly Func<CardReward, CardCreationOptions> GetOptions =
            AccessTools.MethodDelegate<Func<CardReward, CardCreationOptions>>(
                AccessTools.DeclaredPropertyGetter(typeof(CardReward), "Options"));

        private static readonly Func<CardReward, int> GetOptionCount =
            AccessTools.MethodDelegate<Func<CardReward, int>>(
                AccessTools.DeclaredPropertyGetter(typeof(CardReward), "OptionCount"));

        private static void Postfix(CardReward __instance)
        {
            try
            {
                var options = GetOptions(__instance);
                var pools = options.CardPools.Count > 0
                    ? string.Join(",", options.CardPools.Select(pool => pool.Id.ToString()))
                    : "<none>";
                var customPoolCount = options.CustomCardPool?.Count() ?? 0;
                var cards = string.Join(" | ", __instance.Cards.Select(DescribeCard));

                Log.Info(
                    "[KoishiRewardDiagnostics] CardReward populated " +
                    $"player={__instance.Player.NetId} " +
                    $"optionCount={GetOptionCount(__instance)} " +
                    $"source={options.Source} rarityOdds={options.RarityOdds} flags={options.Flags} " +
                    $"pools={pools} customPoolCount={customPoolCount} " +
                    $"cards={cards}");
            }
            catch (Exception ex)
            {
                Log.Error($"[KoishiRewardDiagnostics] Failed to inspect CardReward.Populate: {ex}");
            }
        }

        private static string DescribeCard(CardModel card)
        {
            return $"{card.Id}/{card.Rarity}/{card.Type}/pool={card.Pool.Id}";
        }
    }

    [HarmonyPatch(typeof(NRewardsScreen), nameof(NRewardsScreen.ShowScreen))]
    public static class RewardsScreenDiagnosticsPatch
    {
        private static void Prefix(RewardsSet set, bool isTerminal, IRunState runState)
        {
            try
            {
                Log.Info(
                    "[KoishiRewardDiagnostics] Showing rewards screen " +
                    $"player={set.Player.NetId} terminal={isTerminal} " +
                    $"room={runState.CurrentRoom?.GetType().Name ?? "<null>"} " +
                    $"rewards={string.Join(",", set.Rewards.Select(reward => reward.GetType().Name))}");
            }
            catch (Exception ex)
            {
                Log.Error($"[KoishiRewardDiagnostics] Failed to inspect NRewardsScreen.ShowScreen: {ex}");
            }
        }
    }
}
