using HarmonyLib;
using KomeijiKoishi.Config;
using KomeijiKoishi.Enums;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace KomeijiKoishi.Patches;

public static class BalanceKeywordDescriptionPatch
{
    [HarmonyPatch(typeof(HoverTipFactory), nameof(HoverTipFactory.FromKeyword), typeof(CardKeyword))]
    private static class HoverTipFactoryFromKeywordPatch
    {
        [HarmonyPostfix]
        private static void Postfix(CardKeyword keyword, ref IHoverTip __result)
        {
            if (!KoishiBalanceManager.IsEnabled || keyword != KoishiKeywords.Danmaku)
            {
                return;
            }

            LocString? description = LocString.GetIfExists("card_keywords", "KOMEIJIKOISHI-DANMAKU.balanceDescription");
            if (description == null)
            {
                return;
            }

            LocString title = new("card_keywords", "KOMEIJIKOISHI-DANMAKU.title");
            __result = new HoverTip(title, description, null);
        }
    }
}
