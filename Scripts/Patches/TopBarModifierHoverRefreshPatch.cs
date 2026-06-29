using HarmonyLib;
using KomeijiKoishi.Modifiers;
using MegaCrit.sts2.Core.Nodes.TopBar;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace KomeijiKoishi.Patches
{
    [HarmonyPatch(typeof(NTopBarModifier), "OnFocus")]
    public static class TopBarModifierHoverRefreshPatch
    {
        private static readonly AccessTools.FieldRef<NTopBarModifier, ModifierModel> ModifierRef =
            AccessTools.FieldRefAccess<NTopBarModifier, ModifierModel>("_modifier");

        private static readonly AccessTools.FieldRef<NTopBarModifier, HoverTip> HoverTipRef =
            AccessTools.FieldRefAccess<NTopBarModifier, HoverTip>("_hoverTip");

        public static void Prefix(NTopBarModifier __instance)
        {
            ModifierModel modifier = ModifierRef(__instance);
            if (modifier is not KoishiAncientWeightsModifier)
            {
                return;
            }

            HoverTipRef(__instance) = new HoverTip(modifier.Title, modifier.Description, null);
        }
    }
}
