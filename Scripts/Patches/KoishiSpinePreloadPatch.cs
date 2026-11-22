using Godot;
using HarmonyLib;
using KomeijiKoishi.Characters;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.TestSupport;

namespace KomeijiKoishi.Patches;

[HarmonyPatch(typeof(PreloadManager), nameof(PreloadManager.LoadRunAssets))]
internal static class KoishiSpinePreloadPatch
{
    private static void Prefix(ref IEnumerable<CharacterModel> characters)
    {
        if (!PreloadManager.Enabled || TestMode.IsOn)
            return;

        var runCharacters = characters.ToArray();
        characters = runCharacters;
        var koishi = runCharacters.OfType<KoishiCharacter>().FirstOrDefault();
        if (koishi == null)
            return;

        // Premium scenes share one Spine resource. Load them serially before
        // the game's background loader calculates which assets are missing.
        string[] paths = [koishi.CustomVisualPath, koishi.CustomMerchantAnimPath, koishi.CustomRestSiteAnimPath];
        foreach (string path in paths)
        {
            Log.Info($"[KoishiSpine] Loading {path}");
            var scene = ResourceLoader.Load<PackedScene>(path);
            if (scene == null)
            {
                Log.Warn($"[KoishiSpine] Could not load {path}");
                continue;
            }

            PreloadManager.Cache.SetAsset(path, scene);
            Log.Info($"[KoishiSpine] Loaded {path}");
        }
    }
}
