using System;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using KomeijiKoishi.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace KomeijiKoishi.Patches
{
    [HarmonyPatch(typeof(NCard), "UpdateVisuals")]
    [HarmonyPriority(Priority.Last)]
    public static class DataminerCompositePortraitPatch
    {
        private const int MaxCacheEntries = 32;
        private const string AppliedKeyMeta = "koishi_dataminer_composite_portrait_key";
        private const string TextureMeta = "koishi_dataminer_composite_portrait_texture";

        private static readonly AccessTools.FieldRef<NCard, TextureRect> PortraitRef =
            AccessTools.FieldRefAccess<NCard, TextureRect>("_portrait");

        private static readonly string[] DataminerPortraitPaths =
        {
            "res://mods/Komeiji_Koishi/images/error/DataminerCard_Koishi_1.png",
            "res://mods/Komeiji_Koishi/images/error/DataminerCard_Koishi_2.png",
            "res://mods/Komeiji_Koishi/images/error/DataminerCard_Koishi_3.png",
            "res://mods/Komeiji_Koishi/images/error/DataminerCard_Koishi_4.png",
            "res://mods/Komeiji_Koishi/images/error/DataminerCard_Koishi_5.png",
            "res://mods/Komeiji_Koishi/images/error/DataminerCard_Koishi_6.png",
            "res://mods/Komeiji_Koishi/images/error/DataminerCard_Koishi_7.png"
        };

        private static readonly Dictionary<string, Texture2D> TextureCache = new();
        private static readonly Queue<string> CacheOrder = new();
        private static bool loggedError;

        [HarmonyPostfix]
        public static void Postfix(NCard __instance)
        {
            TryApply(__instance);
        }

        public static void TryApply(NCard __instance)
        {
            try
            {
                if (!GodotObject.IsInstanceValid(__instance))
                {
                    return;
                }

                if (__instance.Model is not DataminerCard_Koishi dataminer)
                {
                    ClearPreviousTexture(__instance);
                    return;
                }

                TextureRect? portrait = PortraitRef(__instance);
                if (portrait == null || !GodotObject.IsInstanceValid(portrait))
                {
                    return;
                }

                string key = dataminer.GetType().FullName ?? dataminer.GetType().Name;
                if (portrait.HasMeta(AppliedKeyMeta)
                    && portrait.GetMeta(AppliedKeyMeta).AsString() == key
                    && portrait.HasMeta(TextureMeta))
                {
                    return;
                }

                Texture2D? composite = GetOrCreateComposite(key);
                if (composite == null)
                {
                    return;
                }

                portrait.Texture = composite;
                portrait.SetMeta(AppliedKeyMeta, key);
                portrait.SetMeta(TextureMeta, composite);
            }
            catch (Exception e)
            {
                if (!loggedError)
                {
                    loggedError = true;
                    GD.PushWarning($"[Koishi] Dataminer composite portrait skipped: {e.Message}");
                }
            }
        }

        private static void ClearPreviousTexture(NCard card)
        {
            TextureRect? portrait = PortraitRef(card);
            if (portrait == null || !GodotObject.IsInstanceValid(portrait))
            {
                return;
            }

            if (!portrait.HasMeta(AppliedKeyMeta))
            {
                return;
            }

            portrait.RemoveMeta(AppliedKeyMeta);
            portrait.RemoveMeta(TextureMeta);
        }

        private static Texture2D? GetOrCreateComposite(string seed)
        {
            if (TextureCache.TryGetValue(seed, out Texture2D? cached)
                && GodotObject.IsInstanceValid(cached))
            {
                return cached;
            }

            if (DataminerPortraitPaths.Length == 0)
            {
                return null;
            }

            int variant = DataminerVariant(seed);
            string path = DataminerPortraitPaths[PositiveModulo(variant, DataminerPortraitPaths.Length)];
            Texture2D? portrait = ResourceLoader.Load<Texture2D>(path);
            if (portrait == null || !GodotObject.IsInstanceValid(portrait))
            {
                return null;
            }

            AddToCache(seed, portrait);
            return portrait;
        }

        private static int DataminerVariant(string seed)
        {
            return seed switch
            {
                "KomeijiKoishi.Cards.DataminerCard_Koishi" => 0,
                "KomeijiKoishi.Cards.DataminerCard_Koishi_2" => 1,
                "KomeijiKoishi.Cards.DataminerCard_Koishi_3" => 2,
                "KomeijiKoishi.Cards.DataminerCard_Koishi_4" => 3,
                "KomeijiKoishi.Cards.DataminerCard_Koishi_5" => 4,
                "KomeijiKoishi.Cards.DataminerCard_Koishi_6" => 5,
                "KomeijiKoishi.Cards.DataminerCard_Koishi_7" => 6,
                _ => 0
            };
        }

        private static void AddToCache(string key, Texture2D texture)
        {
            TextureCache[key] = texture;
            CacheOrder.Enqueue(key);

            while (CacheOrder.Count > MaxCacheEntries)
            {
                string oldKey = CacheOrder.Dequeue();
                if (oldKey != key)
                {
                    TextureCache.Remove(oldKey);
                }
            }
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                uint hash = 2166136261;
                foreach (char character in value)
                {
                    hash ^= character;
                    hash *= 16777619;
                }

                return (int)hash;
            }
        }

        private static int PositiveModulo(int value, int modulus)
        {
            int result = value % modulus;
            return result < 0 ? result + modulus : result;
        }
    }

    [HarmonyPatch(typeof(NCard), "Reload")]
    [HarmonyPriority(Priority.Last)]
    public static class DataminerCompositePortraitReloadPatch
    {
        [HarmonyPostfix]
        public static void Postfix(NCard __instance)
        {
            DataminerCompositePortraitPatch.TryApply(__instance);
        }
    }
}
