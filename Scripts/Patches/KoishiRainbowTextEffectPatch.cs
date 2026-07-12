using HarmonyLib;
using KomeijiKoishi.RichTextTags;
using MegaCrit.Sts2.addons.mega_text;

namespace KomeijiKoishi.Patches
{
    [HarmonyPatch(typeof(MegaRichTextLabel), "InstallEffectsIfNeeded")]
    public static class KoishiRainbowTextEffectPatch
    {
        private static readonly KoishiRainbowTextEffect RainbowEffect = new();
        private static readonly KoishiRainbowStillTextEffect RainbowStillEffect = new();

        public static void Postfix(MegaRichTextLabel __instance)
        {
            if (!__instance.BbcodeEnabled)
            {
                return;
            }

            if (__instance.CustomEffects.Contains(RainbowEffect))
            {
                if (!__instance.CustomEffects.Contains(RainbowStillEffect))
                {
                    __instance.CustomEffects.Add(RainbowStillEffect);
                }

                return;
            }

            __instance.CustomEffects.Add(RainbowEffect);
            __instance.CustomEffects.Add(RainbowStillEffect);
        }
    }
}
