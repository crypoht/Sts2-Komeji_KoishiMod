using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Patches.Content;
using Godot;
using KomeijiKoishi.Ancients;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using System.Reflection;

namespace KomeijiKoishi.Config;

public enum AncientProbabilityId
{
    MoriyaTwoGods,
    HakureiReimu,
    Neow,
    Orobas,
    Tezcatara,
    Pael,
    Darv,
    Nonupeipe,
    Tanx,
    Vakuu
}

public sealed record AncientProbabilityInfo(
    string Key,
    string DisplayName,
    Color Color,
    IReadOnlySet<int> Acts,
    Func<AncientWeights, int> WeightSelector);

public sealed record AncientProbabilityEntry(AncientProbabilityInfo Info, int Weight, float Probability);

public static class AncientProbabilityData
{
    private static readonly Color FallbackColor = Color.FromHtml("#808080");
    private static readonly Dictionary<string, AncientProbabilityInfo> RegisteredInfos = new();
    private static int _registeredCustomAncientCount = -1;

    public static readonly IReadOnlyDictionary<string, Color> Colors =
        new Dictionary<string, Color>
        {
            [AncientProbabilityId.MoriyaTwoGods.ToString()] = Color.FromHtml("#3CB371"),
            [AncientProbabilityId.HakureiReimu.ToString()] = Color.FromHtml("#FF0000"),
            [AncientProbabilityId.Neow.ToString()] = Color.FromHtml("#1E90FF"),
            [AncientProbabilityId.Orobas.ToString()] = Color.FromHtml("#F0F8FF"),
            [AncientProbabilityId.Tezcatara.ToString()] = Color.FromHtml("#FF8C00"),
            [AncientProbabilityId.Pael.ToString()] = Color.FromHtml("#FFDAB9"),
            [AncientProbabilityId.Darv.ToString()] = Color.FromHtml("#9932CC"),
            [AncientProbabilityId.Nonupeipe.ToString()] = Color.FromHtml("#7FFFAA"),
            [AncientProbabilityId.Tanx.ToString()] = Color.FromHtml("#FF4500"),
            [AncientProbabilityId.Vakuu.ToString()] = Color.FromHtml("#800000")
        };

    static AncientProbabilityData()
    {
        RegisterBuiltIns();
    }

    public static IReadOnlyList<AncientProbabilityInfo> AllInfos
    {
        get
        {
            TryRegisterExternalAncients();
            return RegisteredInfos.Values.ToList();
        }
    }

    public static IReadOnlyList<AncientProbabilityInfo> ExternalInfos
    {
        get
        {
            TryRegisterExternalAncients();
            return RegisteredInfos.Values.Where(info => info.Key.StartsWith("External.", StringComparison.Ordinal)).ToList();
        }
    }

    public static void Register(AncientProbabilityInfo info)
    {
        RegisteredInfos[info.Key] = info;
    }

    public static void Register(
        string key,
        string displayName,
        IEnumerable<int> acts,
        Func<AncientWeights, int> weightSelector,
        Color? color = null)
    {
        Register(new AncientProbabilityInfo(
            key,
            displayName,
            color ?? Colors.GetValueOrDefault(key, FallbackColor),
            acts.ToHashSet(),
            weightSelector));
    }

    public static IReadOnlyList<AncientProbabilityEntry> GetActEntries(int actNumber)
    {
        return GetActEntries(actNumber, KoishiModConfig.CurrentAncientWeights);
    }

    public static IReadOnlyList<AncientProbabilityEntry> GetActEntries(int actNumber, AncientWeights weights)
    {
        List<(AncientProbabilityInfo Info, int Weight)> weighted = AllInfos
            .Where(info => info.Acts.Contains(actNumber))
            .Select(info => (info, Math.Max(0, info.WeightSelector(weights))))
            .Where(item => item.Item2 > 0)
            .ToList();

        int totalWeight = weighted.Sum(item => item.Weight);
        if (totalWeight <= 0)
        {
            return Array.Empty<AncientProbabilityEntry>();
        }

        return weighted
            .Select(item => new AncientProbabilityEntry(item.Info, item.Weight, item.Weight / (float)totalWeight))
            .ToList();
    }

    private static void RegisterBuiltIns()
    {
        Register(AncientProbabilityId.MoriyaTwoGods.ToString(), "守矢二神", new[] { 2 }, weights => weights.MoriyaTwoGods);
        Register(AncientProbabilityId.HakureiReimu.ToString(), "博丽灵梦", new[] { 3 }, weights => weights.HakureiReimu);
        Register(AncientProbabilityId.Neow.ToString(), "涅奥", new[] { 1 }, _ => 1);
        Register(AncientProbabilityId.Orobas.ToString(), "欧洛巴斯", new[] { 2 }, weights => weights.Orobas);
        Register(AncientProbabilityId.Tezcatara.ToString(), "特兹卡塔拉", new[] { 2 }, weights => weights.Tezcatara);
        Register(AncientProbabilityId.Pael.ToString(), "佩尔", new[] { 2 }, weights => weights.Pael);
        Register(AncientProbabilityId.Darv.ToString(), "达弗", new[] { 2, 3 }, weights => weights.Darv);
        Register(AncientProbabilityId.Nonupeipe.ToString(), "诺奴佩普", new[] { 3 }, weights => weights.Nonupeipe);
        Register(AncientProbabilityId.Tanx.ToString(), "坦克斯", new[] { 3 }, weights => weights.Tanx);
        Register(AncientProbabilityId.Vakuu.ToString(), "瓦库", new[] { 3 }, weights => weights.Vakuu);
    }

    private static void TryRegisterExternalAncients()
    {
        try
        {
            RegisterExternalAncients();
        }
        catch (Exception e)
        {
            MegaCrit.Sts2.Core.Logging.Log.Error($"[KoishiAncientWeights] Failed to scan external ancients: {e}");
        }
    }

    private static void RegisterExternalAncients()
    {
        if (_registeredCustomAncientCount == CustomContentDictionary.CustomAncients.Count)
        {
            return;
        }

        _registeredCustomAncientCount = CustomContentDictionary.CustomAncients.Count;
        foreach (CustomAncientModel ancient in CustomContentDictionary.CustomAncients)
        {
            try
            {
                if (IsKnownAncient(ancient))
                {
                    continue;
                }

                IReadOnlySet<int> validActNumbers = GetValidActNumbers(ancient);
                if (validActNumbers.Count == 0)
                {
                    continue;
                }

                string key = ExternalKey(ancient);
                Register(
                    key,
                    ResolveAncientName(ancient),
                    validActNumbers,
                    _ => GetExternalWeight(key),
                    FallbackColor);
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[KoishiAncientWeights] Skipping external ancient {SafeAncientDebugName(ancient)}: {e}");
            }
        }
    }

    private static bool IsKnownAncient(AncientEventModel ancient)
    {
        return ancient is MoriyaTwoGods_Koishi
            or HakureiReimu_Koishi
            or Neow
            or Orobas
            or Tezcatara
            or Pael
            or Darv
            or Nonupeipe
            or Tanx
            or Vakuu;
    }

    public static string ExternalKey(AncientEventModel ancient)
    {
        return "External." + SanitizeKey(ancient.Id.Entry);
    }

    public static string ConfigRowKey(string key)
    {
        return SanitizeKey(key);
    }

    private static IReadOnlySet<int> GetValidActNumbers(CustomAncientModel ancient)
    {
        IReadOnlySet<int>? knownActNumbers = GetKnownExternalActNumbers(ancient);
        if (knownActNumbers != null)
        {
            return knownActNumbers;
        }

        return ModelDb.Acts
            .Where(act => IsValidForActSafely(ancient, act))
            .Select(act => act.ActNumber())
            .Where(actNumber => actNumber > 0)
            .ToHashSet();
    }

    private static IReadOnlySet<int>? GetKnownExternalActNumbers(CustomAncientModel ancient)
    {
        string? fullName = ancient.GetType().FullName;
        if (fullName == "KoishiAncient.Ancient.KomeijiKoishi")
        {
            return new HashSet<int> { 2 };
        }

        if (fullName != null && fullName.StartsWith("TouhouAncients.Scripts.", StringComparison.Ordinal))
        {
            try
            {
                PropertyInfo? showActProperty = ancient.GetType().GetProperty("ShowAct", BindingFlags.Instance | BindingFlags.Public);
                if (showActProperty?.GetValue(ancient) is int showAct)
                {
                    return new HashSet<int> { showAct };
                }
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[KoishiAncientWeights] Failed to read TouhouAncients ShowAct for {SafeAncientDebugName(ancient)}: {e}");
            }

            return new HashSet<int> { 2, 3 };
        }

        return null;
    }

    private static bool IsValidForActSafely(CustomAncientModel ancient, ActModel act)
    {
        try
        {
            return ancient.IsValidForAct(act);
        }
        catch (Exception e)
        {
            MegaCrit.Sts2.Core.Logging.Log.Error($"[KoishiAncientWeights] External ancient {SafeAncientDebugName(ancient)} failed IsValidForAct for {act.Id.Entry}: {e}");
            return false;
        }
    }

    private static string ResolveAncientName(AncientEventModel ancient)
    {
        LocString? title = LocString.GetIfExists("ancients", $"{ancient.Id.Entry}.title");
        if (title != null)
        {
            try
            {
                string text = title.GetFormattedText();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }
            catch
            {
                // Mods can ship partial or malformed localization; fall back to a stable id.
            }
        }

        return ancient.Id.Entry;
    }

    private static string SanitizeKey(string rawKey)
    {
        if (string.IsNullOrWhiteSpace(rawKey))
        {
            return "UnknownAncient";
        }

        char[] chars = rawKey
            .Select(ch => char.IsLetterOrDigit(ch) || ch == '_' ? ch : '_')
            .ToArray();
        string sanitized = new(chars);
        return string.IsNullOrWhiteSpace(sanitized) ? "UnknownAncient" : sanitized;
    }

    private static string SafeAncientDebugName(AncientEventModel ancient)
    {
        try
        {
            return $"{ancient.GetType().FullName} ({ancient.Id})";
        }
        catch
        {
            return ancient.GetType().FullName ?? ancient.GetType().Name;
        }
    }

    private static int GetExternalWeight(string key)
    {
        if (KoishiModConfig.ActiveRunExternalAncientWeights.TryGetValue(key, out int activeWeight))
        {
            return int.Clamp(activeWeight, 0, 10);
        }

        return KoishiModConfig.GetExternalAncientWeight(key);
    }
}
