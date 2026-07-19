using Godot;
using HarmonyLib;
using KomeijiKoishi.Utils_Koishi;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;

namespace KomeijiKoishi.Patches;

[HarmonyPatch(typeof(NHandCardHolder), nameof(NHandCardHolder.UpdateCard))]
public static class UnconsciousCardGlowPatch
{
    private static readonly Color PurpleGlow = new(0.72f, 0.18f, 1f, 0.98f);

    [HarmonyPostfix]
    public static void Postfix(NHandCardHolder __instance)
    {
        NCard? cardNode = __instance.CardNode;
        if (cardNode?.Model == null || cardNode.CardHighlight == null)
        {
            return;
        }

        if (!CombatManager.Instance.IsInProgress || !KoishiExtensions.IsTrulyUnconscious(cardNode.Model))
        {
            return;
        }

        cardNode.CardHighlight.AnimShow();
        cardNode.CardHighlight.Modulate = PurpleGlow;
    }
}
