using HarmonyLib;
using KomeijiKoishi.RichTextTags;
using MegaCrit.Sts2.Core.RichTextTags;
using MegaCrit.Sts2.addons.mega_text;

namespace KomeijiKoishi.Patches
{
    [HarmonyPatch(typeof(MegaRichTextLabel), "InstallEffectsIfNeeded")]
    public static class KoishiRainbowTextEffectPatch
    {
        public static void Postfix(MegaRichTextLabel __instance)
        {
            KoishiRainbowTextEffectInstaller.EnsureInstalled(__instance);
        }
    }

    [HarmonyPatch(typeof(MegaRichTextLabel), "SetTextAutoSize")]
    public static class KoishiRainbowTextEffectSetTextPatch
    {
        public static void Postfix(MegaRichTextLabel __instance)
        {
            KoishiRainbowTextEffectInstaller.EnsureInstalled(__instance);

            if (__instance.BbcodeEnabled)
            {
                __instance.ParseBbcode(__instance.Text);
            }
        }
    }

    internal static class KoishiRainbowTextEffectInstaller
    {
        public static void EnsureInstalled(MegaRichTextLabel label)
        {
            if (!label.BbcodeEnabled)
            {
                return;
            }

            AddIfMissing<KoishiRainbowTextEffect>(label);
            AddIfMissing<KoishiRainbowStillTextEffect>(label);
        }

        private static void AddIfMissing<T>(MegaRichTextLabel label)
            where T : AbstractMegaRichTextEffect, new()
        {
            foreach (var effect in label.CustomEffects)
            {
                if (effect is T)
                {
                    return;
                }
            }

            label.CustomEffects.Add(new T());
        }
    }
}
