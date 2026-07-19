using BaseLib.Patches.Localization;
using KomeijiKoishi.Config;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using System.Text.RegularExpressions;

namespace KomeijiKoishi.Patches;

public static class BalanceCardDescriptionPatch
{
    private const string BalanceDescriptionSuffix = ".balanceDescription";
    private const string KoishiTextEnergyIconPath = "res://mods/Komeiji_Koishi/images/energy_koishi_small.png";
    private static bool _registered;

    private static readonly HashSet<string> BalanceDescriptionCards = new()
    {
        "KOMEIJIKOISHI-AIR_STRIKE_KOISHI",
        "KOMEIJIKOISHI-BRAMBLY_ROSE_GARDEN_KOISHI",
        "KOMEIJIKOISHI-FLAW_OF_DN_A_KOISHI",
        "KOMEIJIKOISHI-INSTINCTIVE_FORM_KOISHI",
        "KOMEIJIKOISHI-IN_FREEDOM_KOISHI",
        "KOMEIJIKOISHI-KOMEIJI_SPIN_KOISHI",
        "KOMEIJIKOISHI-KUUGA_STRIKE_KOISHI",
        "KOMEIJIKOISHI-REIMU_HELP_KOISHI",
        "KOMEIJIKOISHI-SPIRITUAL_DESTRUCTION_KOISHI",
        "KOMEIJIKOISHI-ULTIMATE_UNCONSCIOUS_FORM_KOISHI",
        "KOMEIJIKOISHI-UNINTENTIONAL_HARVEST_KOISHI",
        "KOMEIJIKOISHI-UNCONSCIOUS_GENE_KOISHI",
        "KOMEIJIKOISHI-IDS_SHIKIGAMI_KOISHI"
    };

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        DescriptionOverrides.CustomizeDescription += ReplaceBalanceDescription;
        DescriptionOverrides.CustomizeDescriptionPost += ReplaceRuntimeDescriptionValues;
        _registered = true;
    }

    private static void ReplaceBalanceDescription(CardModel card, Creature? target, ref string description)
    {
        if (!KoishiBalanceManager.IsEnabled || !BalanceDescriptionCards.Contains(card.Id.Entry))
        {
            return;
        }

        LocString? balanceDescription = LocString.GetIfExists("cards", card.Id.Entry + BalanceDescriptionSuffix);
        if (balanceDescription == null)
        {
            return;
        }

        description = FormatBalanceDescription(card, balanceDescription);
    }

    private static string FormatBalanceDescription(CardModel card, LocString locString)
    {
        string text = locString.GetRawText();

        text = Regex.Replace(
            text,
            @"\{IfUpgraded:show:(?<body>.*?)\}",
            match =>
            {
                string body = match.Groups["body"].Value;
                string[] parts = body.Split('|', 2);
                return card.IsUpgraded
                    ? parts[0]
                    : parts.Length > 1 ? parts[1] : string.Empty;
            },
            RegexOptions.Singleline);

        text = Regex.Replace(
            text,
            @"\{energyPrefix:energyIcons\((?<count>\d+)\)\}",
            match => FormatEnergyIcons(match.Groups["count"].Value));

        text = Regex.Replace(
            text,
            @"\{(?<name>[A-Za-z0-9_]+):(?<format>inverseDiff|diff)\(\)\}",
            match =>
            {
                string name = match.Groups["name"].Value;
                string format = match.Groups["format"].Value;
                return card.DynamicVars.TryGetValue(name, out var dynamicVar)
                    ? dynamicVar.ToHighlightedString(format == "inverseDiff")
                    : match.Value;
            });

        return text;
    }

    private static string FormatEnergyIcons(string countText)
    {
        if (!int.TryParse(countText, out int count) || count <= 0)
        {
            return string.Empty;
        }

        return string.Concat(Enumerable.Repeat($"[img]{KoishiTextEnergyIconPath}[/img]", count));
    }

    private static void ReplaceRuntimeDescriptionValues(CardModel card, Creature? target, ref string description)
    {
        if (card.Id.Entry != "KOMEIJIKOISHI-MENTAL_STELLAR_SUCCESSION_KOISHI")
        {
            return;
        }

        int baseValue = KoishiBalanceManager.Value(100, 50);
        int upgradeValue = KoishiBalanceManager.Value(50, 25);
        int multiplier = baseValue + upgradeValue * card.CurrentUpgradeLevel;
        description = Regex.Replace(description, @"\d+%", multiplier + "%");
    }
}
