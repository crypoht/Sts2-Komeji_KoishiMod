#if STS2_BETA
using System;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using KomeijiKoishi.Characters;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;

namespace KomeijiKoishi.Patches
{
    [HarmonyPatch(typeof(NBestiary), "CreateFilters")]
    public static class KoishiBestiaryFilterPatch
    {
        private static readonly MethodInfo? AddFilterMethod = AccessTools.Method(typeof(NBestiary), "AddFilter");
        private static readonly FieldInfo? FilterContainerField = AccessTools.Field(typeof(NBestiary), "_filterContainer");

        [HarmonyPostfix]
        public static void Postfix(NBestiary __instance)
        {
            Control? filterContainer = FilterContainerField?.GetValue(__instance) as Control;
            if (filterContainer == null || AddFilterMethod == null)
            {
                return;
            }

            bool alreadyAdded = filterContainer.GetChildren(false)
                .OfType<NBestiaryCharacterFilter>()
                .Any(filter => filter.character is KoishiCharacter);
            if (alreadyAdded)
            {
                return;
            }

            AddFilterMethod.Invoke(__instance, new object?[] { ModelDb.Character<KoishiCharacter>() });
        }
    }

    [HarmonyPatch(typeof(NBestiary), "DisplayCharacterData")]
    public static class KoishiBestiaryQuotePatch
    {
        private const string KoishiCharacterId = "KOMEIJIKOISHI-KOISHI_CHARACTER";

        private static readonly FieldInfo? CurrentFilterField = AccessTools.Field(typeof(NBestiary), "_currentFilter");
        private static readonly FieldInfo? SelectedEntryField = AccessTools.Field(typeof(NBestiary), "_selectedEntry");
        private static readonly FieldInfo? DialogueLabelField = AccessTools.Field(typeof(NBestiary), "_dialogueLabel");

        [HarmonyPostfix]
        public static void Postfix(NBestiary __instance)
        {
            NBestiaryCharacterFilter? currentFilter = CurrentFilterField?.GetValue(__instance) as NBestiaryCharacterFilter;
            if (currentFilter?.character is not KoishiCharacter)
            {
                return;
            }

            NBestiaryEntry? selectedEntry = SelectedEntryField?.GetValue(__instance) as NBestiaryEntry;
            MonsterModel? monster = selectedEntry?.Entry.monsterModel;
            MegaRichTextLabel? dialogueLabel = DialogueLabelField?.GetValue(__instance) as MegaRichTextLabel;
            if (monster == null || dialogueLabel == null)
            {
                return;
            }

            string prefix = currentFilter.kills <= 0 ? "bestiaryQuote" : "bestiaryKillQuote";
            string monsterId = monster.Id.Entry;
            LocString loc = LocString.GetIfExists("characters", $"{KoishiCharacterId}.{prefix}.{monsterId}")
                ?? BuildUnlistedQuote(prefix, monster);
            AddMonsterVariables(loc, monster);

            dialogueLabel.SetTextAutoSize(loc.GetFormattedText());
        }

        private static LocString BuildUnlistedQuote(string prefix, MonsterModel monster)
        {
            LocString loc = LocString.GetIfExists("characters", $"{KoishiCharacterId}.{prefix}.UNLISTED")
                ?? new LocString("characters", $"{KoishiCharacterId}.bestiaryQuote.UNLISTED");
            return loc;
        }

        private static void AddMonsterVariables(LocString loc, MonsterModel monster)
        {
            loc.Add("MonsterId", monster.Id.Entry);
            loc.Add("MonsterName", monster.Title.GetFormattedText());
            loc.Add("MonsterClass", StringHelper.Unslugify(monster.Id.Entry));
        }
    }

    [HarmonyPatch(typeof(NBestiary), "RefreshStatisticsText")]
    public static class KoishiBestiaryFilterUnlockPatch
    {
        private static readonly FieldInfo? SelectedEntryField = AccessTools.Field(typeof(NBestiary), "_selectedEntry");
        private static readonly FieldInfo? FilterContainerField = AccessTools.Field(typeof(NBestiary), "_filterContainer");

        [HarmonyPostfix]
        public static void Postfix(NBestiary __instance)
        {
            NBestiaryEntry? selectedEntry = SelectedEntryField?.GetValue(__instance) as NBestiaryEntry;
            if (selectedEntry?.Entry.monsterModel == null || !selectedEntry.IsDiscovered)
            {
                return;
            }

            Control? filterContainer = FilterContainerField?.GetValue(__instance) as Control;
            if (filterContainer == null)
            {
                return;
            }

            foreach (NBestiaryCharacterFilter filter in filterContainer.GetChildren(false).OfType<NBestiaryCharacterFilter>())
            {
                if (filter.character is KoishiCharacter)
                {
                    filter.IsLocked = false;
                    return;
                }
            }
        }
    }
}
#endif
