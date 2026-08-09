using Godot;
using HarmonyLib;
using KomeijiKoishi.Cards;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace KomeijiKoishi.Patches
{
    [HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
    [HarmonyPriority(Priority.Last)]
    public static class ArtworkEventBannerPatch
    {
        private const string UncommonBannerMaterialPath = "res://materials/cards/banners/card_banner_uncommon_mat.tres";
        private const string RareBannerMaterialPath = "res://materials/cards/banners/card_banner_rare_mat.tres";

        [HarmonyPostfix]
        public static void Postfix(NCard __instance)
        {
            if (!GodotObject.IsInstanceValid(__instance) || __instance.Model == null)
            {
                return;
            }

            string? materialPath = null;
            Color? titleOutlineColor = null;

            if (__instance.Model is TrueArtwork_Koishi)
            {
                materialPath = UncommonBannerMaterialPath;
                titleOutlineColor = StsColors.cardTitleOutlineUncommon;
            }
            else if (__instance.Model is TrueArtworkLiberated_Koishi)
            {
                materialPath = RareBannerMaterialPath;
                titleOutlineColor = StsColors.cardTitleOutlineRare;
            }

            if (materialPath == null)
            {
                return;
            }

            Material? material = PreloadManager.Cache.GetMaterial(materialPath);
            if (material == null)
            {
                return;
            }

            SetMaterial<TextureRect>(__instance, "_banner", material);
            SetMaterial<TextureRect>(__instance, "_portraitBorder", material);
            SetMaterial<NinePatchRect>(__instance, "_typePlaque", material);
            if (titleOutlineColor.HasValue && __instance.Model.CurrentUpgradeLevel == 0)
            {
                SetTitleOutlineColor(__instance, titleOutlineColor.Value);
            }
        }

        private static void SetMaterial<T>(NCard card, string fieldName, Material material)
            where T : CanvasItem
        {
            if (AccessTools.Field(typeof(NCard), fieldName)?.GetValue(card) is T node
                && GodotObject.IsInstanceValid(node))
            {
                node.Material = material;
            }
        }

        private static void SetTitleOutlineColor(NCard card, Color color)
        {
            if (AccessTools.Field(typeof(NCard), "_titleLabel")?.GetValue(card) is Label label
                && GodotObject.IsInstanceValid(label))
            {
                label.AddThemeColorOverride(ThemeConstants.Label.FontOutlineColor, color);
            }
        }
    }
}
