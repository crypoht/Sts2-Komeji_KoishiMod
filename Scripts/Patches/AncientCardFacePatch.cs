using Godot;
using HarmonyLib;
using KomeijiKoishi.Cards;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace KomeijiKoishi.Patches
{
    [HarmonyPatch(typeof(NCard), "UpdateVisuals")]
    [HarmonyPriority(Priority.Last)]
    public static class AncientCardFacePatch
    {
        private const string AppliedMetaKey = "koishi_ancient_card_face_applied";

        private static readonly string AncientBorderPath =
            ImageHelper.GetImagePath("atlases/compressed_atlas.sprites/ancient_card_border.png.tres");

        [HarmonyPostfix]
        public static void Postfix(NCard __instance)
        {
            if (__instance.Model == null)
            {
                return;
            }

            bool shouldApply = KoishiModConfig.UseAncientCardArt && __instance.Model is IUseAncientCardFace;
            if (!shouldApply)
            {
                if (__instance.HasMeta(AppliedMetaKey))
                {
                    __instance.RemoveMeta(AppliedMetaKey);
                    RestoreOfficialFace(__instance);
                }
                return;
            }

            ApplyAncientFace(
                __instance,
                Field<TextureRect>(__instance, "_portrait"),
                Field<TextureRect>(__instance, "_portraitBorder"),
                Field<TextureRect>(__instance, "_frame"),
                Field<TextureRect>(__instance, "_ancientPortrait"),
                Field<TextureRect>(__instance, "_ancientBorderGlassOverlay"),
                Field<TextureRect>(__instance, "_ancientBorder"),
                Field<TextureRect>(__instance, "_ancientTextBg"),
                Field<Control>(__instance, "_ancientBanner"),
                Field<TextureRect>(__instance, "_banner"),
                Field<CanvasGroup>(__instance, "_portraitCanvasGroup"),
                KoishiImagePaths.CardAncientPortrait(__instance.Model.GetType()));
        }

        private static T? Field<T>(NCard card, string fieldName)
            where T : class
        {
            return AccessTools.Field(typeof(NCard), fieldName)?.GetValue(card) as T;
        }

        private static void ApplyAncientFace(
            NCard card,
            TextureRect? portrait,
            TextureRect? portraitBorder,
            TextureRect? frame,
            TextureRect? ancientPortrait,
            TextureRect? ancientBorderGlassOverlay,
            TextureRect? ancientBorder,
            TextureRect? ancientTextBg,
            Control? ancientBanner,
            TextureRect? banner,
            CanvasGroup? portraitCanvasGroup,
            string texturePath)
        {
            card.SetMeta(AppliedMetaKey, true);

            if (portrait != null)
            {
                portrait.Visible = false;
            }
            if (portraitBorder != null)
            {
                portraitBorder.Visible = false;
            }
            if (frame != null)
            {
                frame.Visible = false;
            }
            if (ancientPortrait != null)
            {
                ancientPortrait.Visible = true;
                ancientPortrait.Material = null;
                ancientPortrait.StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered;
                ancientPortrait.Texture = ResourceLoader.Exists(texturePath)
                    ? ResourceLoader.Load<Texture2D>(texturePath)
                    : card.Model!.Portrait;
            }
            if (ancientBorderGlassOverlay != null)
            {
                ancientBorderGlassOverlay.Visible = true;
            }
            if (ancientBorder != null)
            {
                ancientBorder.Visible = true;
                ancientBorder.Texture = ResourceLoader.Load<Texture2D>(AncientBorderPath, null, ResourceLoader.CacheMode.Reuse);
            }
            if (ancientTextBg != null)
            {
                ancientTextBg.Visible = true;
                ancientTextBg.Texture = ResourceLoader.Load<Texture2D>(GetAncientTextBgPath(card.Model!.Type), null, ResourceLoader.CacheMode.Reuse);
            }
            if (ancientBanner != null)
            {
                ancientBanner.Visible = false;
            }
            if (banner != null)
            {
                banner.Visible = true;
            }
            ApplyOfficialAncientMask(card, portraitCanvasGroup);
        }

        private static void RestoreOfficialFace(NCard card)
        {
            bool isAncient = card.Model?.Rarity == CardRarity.Ancient;

            SetVisible<TextureRect>(card, "_portraitBorder", !isAncient);
            SetVisible<TextureRect>(card, "_portrait", !isAncient);
            SetVisible<TextureRect>(card, "_frame", !isAncient);
            SetVisible<TextureRect>(card, "_ancientPortrait", isAncient);
            SetVisible<TextureRect>(card, "_ancientBorderGlassOverlay", isAncient);
            SetVisible<TextureRect>(card, "_ancientBorder", isAncient);
            SetVisible<TextureRect>(card, "_ancientTextBg", isAncient);
            SetVisible<Control>(card, "_ancientBanner", isAncient);
            SetVisible<TextureRect>(card, "_banner", !isAncient);
        }

        private static void SetVisible<T>(NCard card, string fieldName, bool visible)
            where T : CanvasItem
        {
            T? node = Field<T>(card, fieldName);
            if (node != null)
            {
                node.Visible = visible;
            }
        }

        private static void ApplyOfficialAncientMask(NCard card, CanvasGroup? portraitCanvasGroup)
        {
            if (portraitCanvasGroup == null)
            {
                return;
            }

            Material? maskMaterial = Field<Material>(card, "_canvasGroupMaskMaterial");
            if (maskMaterial == null)
            {
                maskMaterial = PreloadManager.Cache.GetMaterial("res://scenes/cards/card_canvas_group_mask_material.tres");
                AccessTools.Field(typeof(NCard), "_canvasGroupMaskMaterial")?.SetValue(card, maskMaterial);
            }
            portraitCanvasGroup.Material = maskMaterial;
        }

        private static string GetAncientTextBgPath(CardType cardType)
        {
            CardType textBgType = cardType switch
            {
                CardType.Attack or CardType.Skill or CardType.Power or CardType.Quest => cardType,
                _ => CardType.Skill
            };

            return ImageHelper.GetImagePath("atlases/compressed_atlas.sprites/ancient_text_bg_" + textBgType.ToString().ToLowerInvariant() + ".png.tres");
        }
    }
}
