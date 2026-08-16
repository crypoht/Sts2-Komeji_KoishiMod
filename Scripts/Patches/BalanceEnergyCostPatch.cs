using HarmonyLib;
using KomeijiKoishi.Config;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace KomeijiKoishi.Patches;

[HarmonyPatch(typeof(CardEnergyCost), nameof(CardEnergyCost.GetWithModifiers))]
public static class BalanceEnergyCostPatch
{
    private static readonly AccessTools.FieldRef<CardEnergyCost, CardModel> CardRef =
        AccessTools.FieldRefAccess<CardEnergyCost, CardModel>("_card");

    private static readonly Dictionary<string, (int Normal, int Balanced)> BalanceCosts = new()
    {
        ["KOMEIJIKOISHI-ANCESTORS_DREAM_KOISHI"] = (1, 2),
        ["KOMEIJIKOISHI-BRAMBLY_ROSE_GARDEN_KOISHI"] = (2, 1),
        ["KOMEIJIKOISHI-BURIED_FIRE_KOISHI"] = (2, 3),
        ["KOMEIJIKOISHI-CONSCIOUSNESS_SPIRAL_KOISHI"] = (2, 3),
        ["KOMEIJIKOISHI-SELF_OVERFLOW_KOISHI"] = (0, 0),
        ["KOMEIJIKOISHI-SUPEREGO_KOISHI"] = (3, 4),
        ["KOMEIJIKOISHI-UNCONSCIOUS_DANMAKU_ATTACK_KOISHI"] = (2, 3),
        ["KOMEIJIKOISHI-VOID_EXPANSION_KOISHI"] = (0, 1)
    };

    public static void Postfix(CardEnergyCost __instance, CostModifiers modifiers, ref int __result)
    {
        if (__instance.CostsX || __instance.HasLocalModifiers)
        {
            return;
        }

        CardModel? card = CardRef(__instance);
        if (card == null || !BalanceCosts.TryGetValue(card.Id.Entry, out var costs))
        {
            return;
        }

        int targetBaseCost = KoishiBalanceManager.IsEnabled ? costs.Balanced : costs.Normal;
        int delta = targetBaseCost - __instance.Canonical;
        if (delta == 0)
        {
            return;
        }

        __result = Math.Max(0, __result + delta);
    }
}
