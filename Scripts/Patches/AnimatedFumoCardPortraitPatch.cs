using Godot;
using HarmonyLib;
using KomeijiKoishi.Cards;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace KomeijiKoishi.Patches
{
    public static class AnimatedFumoCardPortraitPatch
    {
        private const string TweenMetaKey = "koishi_animated_fumo_portrait_tween";
        private const string DeferredMetaKey = "koishi_animated_fumo_portrait_deferred";

        private sealed record AnimationConfig(
            Type CardType,
            string FrameDirectory,
            int FrameCount,
            double FrameDuration);

        private static readonly AnimationConfig[] Animations =
        {
            new(
                typeof(SprinkleStarHeart_Koishi),
                "res://mods/Komeiji_Koishi/images/cards/animated/SprinkleStarHeart_Koishi_fumo/",
                33,
                0.13d),
            new(
                typeof(JoyfulDance_Koishi),
                "res://mods/Komeiji_Koishi/images/cards/animated/JoyfulDance_Koishi_fumo/",
                54,
                0.0835d),
            new(
                typeof(UnconsciousExplosion_Koishi),
                "res://mods/Komeiji_Koishi/images/cards/animated/UnconsciousExplosion_Koishi_fumo/",
                60,
                0.05d),
            new(
                typeof(LookingGlassSelf_Koishi),
                "res://mods/Komeiji_Koishi/images/cards/animated/LookingGlassSelf_Koishi_fumo/",
                70,
                0.07d),
            new(
                typeof(DanmakuEndlessMiracle_Koishi),
                "res://mods/Komeiji_Koishi/images/cards/animated/DanmakuEndlessMiracle_Koishi_fumo/",
                18,
                0.07d)
        };

        private static readonly AccessTools.FieldRef<NCard, TextureRect> PortraitRef =
            AccessTools.FieldRefAccess<NCard, TextureRect>("_portrait");

        private static readonly AccessTools.FieldRef<NCard, TextureRect> AncientPortraitRef =
            AccessTools.FieldRefAccess<NCard, TextureRect>("_ancientPortrait");

        public static void TryApply(NCard card)
        {
            if (!KoishiModConfig.UseFumoCardArt || card.Model == null || !TryGetAnimation(card.Model.GetType(), out AnimationConfig? animation))
            {
                return;
            }

            AnimatePortrait(card, PortraitRef(card), animation!);
            AnimatePortrait(card, AncientPortraitRef(card), animation!);
        }

        private static void AnimatePortrait(NCard card, TextureRect? portrait, AnimationConfig animation)
        {
            if (portrait == null)
            {
                return;
            }

            if (!portrait.IsInsideTree())
            {
                ScheduleDeferredApply(card);
                return;
            }

            if (portrait.HasMeta(TweenMetaKey))
            {
                Tween? oldTween = portrait.GetMeta(TweenMetaKey).AsGodotObject() as Tween;
                if (oldTween != null && GodotObject.IsInstanceValid(oldTween))
                {
                    oldTween.Kill();
                }
            }

            Tween tween = portrait.CreateTween();
            portrait.SetMeta(TweenMetaKey, tween);
            tween.SetLoops();
            for (int i = 0; i < animation.FrameCount; i++)
            {
                Texture2D frame = PreloadManager.Cache.GetCompressedTexture2D(GetFramePath(animation, i));
                tween.TweenCallback(Callable.From(() =>
                {
                    if (GodotObject.IsInstanceValid(card) && card.Model != null && card.Model.GetType() == animation.CardType && KoishiModConfig.UseFumoCardArt)
                    {
                        portrait.Texture = frame;
                    }
                }));
                tween.TweenInterval(animation.FrameDuration);
            }
        }

        private static bool TryGetAnimation(Type cardType, out AnimationConfig? animation)
        {
            foreach (AnimationConfig config in Animations)
            {
                if (config.CardType == cardType)
                {
                    animation = config;
                    return true;
                }
            }

            animation = null;
            return false;
        }

        private static string GetFramePath(AnimationConfig animation, int frameIndex)
        {
            return animation.FrameDirectory + $"frame_{frameIndex:000}.png";
        }

        private static void ScheduleDeferredApply(NCard card)
        {
            if (!GodotObject.IsInstanceValid(card) || card.HasMeta(DeferredMetaKey))
            {
                return;
            }

            card.SetMeta(DeferredMetaKey, true);
            Callable.From(() => DeferredApply(card)).CallDeferred();
        }

        public static void DeferredApply(NCard card)
        {
            if (!GodotObject.IsInstanceValid(card))
            {
                return;
            }

            card.RemoveMeta(DeferredMetaKey);
            TryApply(card);
        }
    }

    [HarmonyPatch(typeof(NCard), "Reload")]
    public static class AnimatedFumoCardPortraitReloadPatch
    {
        [HarmonyPostfix]
        public static void Postfix(NCard __instance)
        {
            AnimatedFumoCardPortraitPatch.TryApply(__instance);
        }
    }

    [HarmonyPatch(typeof(NCard), "_Ready")]
    public static class AnimatedFumoCardPortraitReadyPatch
    {
        [HarmonyPostfix]
        public static void Postfix(NCard __instance)
        {
            AnimatedFumoCardPortraitPatch.TryApply(__instance);
        }
    }

    [HarmonyPatch(typeof(NCard), "UpdateVisuals")]
    public static class AnimatedFumoCardPortraitUpdateVisualsPatch
    {
        [HarmonyPostfix]
        public static void Postfix(NCard __instance)
        {
            AnimatedFumoCardPortraitPatch.TryApply(__instance);
        }
    }
}
