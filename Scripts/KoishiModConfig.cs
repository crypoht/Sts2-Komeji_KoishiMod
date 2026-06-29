using BaseLib.Config;
using BaseLib.Config.UI;
using Godot;
using KomeijiKoishi.Config;
using KomeijiKoishi.Modifiers;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using System.Linq;

namespace KomeijiKoishi;

[ConfigHoverTipsByDefault]
public sealed class KoishiModConfig : SimpleModConfig
{
    private const int DefaultAncientWeight = 3;
    private AncientProbabilityChart? _ancientProbabilityChart;
    private HSeparator? _baseAncientWeightsSeparator;

    [ConfigSection("AncientSettings")]
    public static bool EnableAncients { get; set; } = true;

    [ConfigSection("VisualSettings")]
    public static bool UseFumoCardArt { get; set; } = false;

    public static bool PlayMoriyaDanceForAllPlayers { get; set; } = false;

    [ConfigSection("AncientWeights")]
    public static bool EnableAncientWeights { get; set; } = false;

    [ConfigVisibleIf(nameof(ShouldShowKoishiAncientWeight))]
    [ConfigSlider(0, 10, 1)]
    public static int MoriyaTwoGodsWeight { get; set; } = DefaultAncientWeight;

    [ConfigVisibleIf(nameof(ShouldShowKoishiAncientWeight))]
    [ConfigSlider(0, 10, 1)]
    public static int HakureiReimuWeight { get; set; } = DefaultAncientWeight;

    [ConfigVisibleIf(nameof(EnableAncientWeights))]
    [ConfigSlider(0, 10, 1)]
    public static int OrobasWeight { get; set; } = DefaultAncientWeight;

    [ConfigVisibleIf(nameof(EnableAncientWeights))]
    [ConfigSlider(0, 10, 1)]
    public static int PaelWeight { get; set; } = DefaultAncientWeight;

    [ConfigVisibleIf(nameof(EnableAncientWeights))]
    [ConfigSlider(0, 10, 1)]
    public static int TezcataraWeight { get; set; } = DefaultAncientWeight;

    [ConfigVisibleIf(nameof(EnableAncientWeights))]
    [ConfigSlider(0, 10, 1)]
    public static int NonupeipeWeight { get; set; } = DefaultAncientWeight;

    [ConfigVisibleIf(nameof(EnableAncientWeights))]
    [ConfigSlider(0, 10, 1)]
    public static int TanxWeight { get; set; } = DefaultAncientWeight;

    [ConfigVisibleIf(nameof(EnableAncientWeights))]
    [ConfigSlider(0, 10, 1)]
    public static int VakuuWeight { get; set; } = DefaultAncientWeight;

    [ConfigVisibleIf(nameof(EnableAncientWeights))]
    [ConfigSlider(0, 10, 1)]
    public static int DarvWeight { get; set; } = DefaultAncientWeight;

    [ConfigHideInUI]
    public static string ExternalAncientWeights { get; set; } = "";

    [ConfigIgnore]
    public static bool ActiveRunAncientsEnabled { get; private set; } = true;

    [ConfigIgnore]
    public static AncientWeights ActiveRunAncientWeights { get; private set; } = AncientWeights.Default;

    [ConfigIgnore]
    public static IReadOnlyDictionary<string, int> ActiveRunExternalAncientWeights { get; private set; } = new Dictionary<string, int>();

    [ConfigIgnore]
    public static int ActiveRunCurrentActIndex { get; private set; }

    public override void SetupConfigUI(Control optionContainer)
    {
        base.SetupConfigUI(optionContainer);
        try
        {
            AddAncientProbabilityChart(optionContainer);
        }
        catch (Exception e)
        {
            MegaCrit.Sts2.Core.Logging.Log.Error($"[KoishiAncientWeights] Failed to add ancient weight UI: {e}");
        }

        try
        {
            SetupFocusNeighbors(optionContainer);
        }
        catch (Exception e)
        {
            MegaCrit.Sts2.Core.Logging.Log.Error($"[KoishiConfig] Failed to setup focus neighbors: {e}");
        }
    }

    public static void BeginRunWithHostConfig(IReadOnlyList<ModifierModel> modifiers)
    {
        ActiveRunAncientsEnabled = !modifiers.Any(m => m is DisableKoishiAncientsModifier);
        ActiveRunAncientWeights = modifiers.Any(m => m is DisableKoishiAncientWeightsModifier)
            ? AncientWeights.Default
            : modifiers.OfType<KoishiAncientWeightsModifier>().FirstOrDefault()?.ToAncientWeights() ?? AncientWeights.Default;
        ActiveRunExternalAncientWeights = modifiers.Any(m => m is DisableKoishiAncientWeightsModifier)
            ? new Dictionary<string, int>()
            : modifiers.OfType<KoishiAncientWeightsModifier>().FirstOrDefault()?.ExternalWeights ?? new Dictionary<string, int>();
        ActiveRunCurrentActIndex = 0;
    }

    public static void BeginRunWithHostConfig(RunState state)
    {
        BeginRunWithHostConfig(state.Modifiers);
        ActiveRunCurrentActIndex = state.CurrentActIndex;
    }

    public static bool HasRunAncientWeights(IRunState? runState)
    {
        return GetRunAncientWeightsModifier(runState) != null;
    }

    public static bool AreAncientWeightsEnabledForRun(IRunState? runState)
    {
        if (runState == null)
        {
            return EnableAncients && EnableAncientWeights;
        }

        IReadOnlyList<ModifierModel> modifiers = runState.Modifiers;
        return !modifiers.Any(m => m is DisableKoishiAncientWeightsModifier)
            && modifiers.Any(m => m is KoishiAncientWeightsModifier);
    }

    public static AncientWeights GetAncientWeightsForRun(IRunState? runState)
    {
        KoishiAncientWeightsModifier? modifier = GetRunAncientWeightsModifier(runState);
        return modifier?.ToAncientWeights() ?? ActiveRunAncientWeights;
    }

    public static IReadOnlyDictionary<string, int> GetExternalAncientWeightsForRun(IRunState? runState)
    {
        KoishiAncientWeightsModifier? modifier = GetRunAncientWeightsModifier(runState);
        return modifier?.ExternalWeights ?? ActiveRunExternalAncientWeights;
    }

    public static void SetActiveRunCurrentActIndex(int currentActIndex)
    {
        ActiveRunCurrentActIndex = Math.Max(ActiveRunCurrentActIndex, currentActIndex);
    }

    public static bool AreAncientsEnabledForRun(IRunState? runState)
    {
        if (runState == null)
        {
            return ActiveRunAncientsEnabled;
        }

        return !runState.Modifiers.Any(m => m is DisableKoishiAncientsModifier);
    }

    public static IReadOnlyList<ModifierModel> WithHostAncientConfig(IReadOnlyList<ModifierModel> modifiers)
    {
        List<ModifierModel> result = modifiers.Where(m => m is not KoishiAncientWeightsModifier and not DisableKoishiAncientWeightsModifier).ToList();
        bool hasMarker = result.Any(m => m is DisableKoishiAncientsModifier);
        if (EnableAncients)
        {
            if (hasMarker)
            {
                result.RemoveAll(m => m is DisableKoishiAncientsModifier);
            }

        }
        else if (!hasMarker)
        {
            result.Add(ModelDb.Modifier<DisableKoishiAncientsModifier>().ToMutable());
        }

        if (EnableAncientWeights)
        {
            result.Add(CreateWeightsModifier());
        }
        else if (!result.Any(m => m is DisableKoishiAncientWeightsModifier))
        {
            result.Add(ModelDb.Modifier<DisableKoishiAncientWeightsModifier>().ToMutable());
        }

        return result;
    }

    private static KoishiAncientWeightsModifier CreateWeightsModifier()
    {
        KoishiAncientWeightsModifier modifier = ModelDb.Modifier<KoishiAncientWeightsModifier>().ToMutable() as KoishiAncientWeightsModifier
            ?? new KoishiAncientWeightsModifier();
        AncientWeights weights = CurrentAncientWeights;
        modifier.Apply(weights, CurrentExternalAncientWeights());
        return modifier;
    }

    public static AncientWeights CurrentAncientWeights => new(
        EnableAncients ? MoriyaTwoGodsWeight : 0,
        EnableAncients ? HakureiReimuWeight : 0,
        OrobasWeight,
        PaelWeight,
        TezcataraWeight,
        NonupeipeWeight,
        TanxWeight,
        VakuuWeight,
        DarvWeight);

    private static KoishiAncientWeightsModifier? GetRunAncientWeightsModifier(IRunState? runState)
    {
        if (runState == null)
        {
            return null;
        }

        if (runState.Modifiers.Any(m => m is DisableKoishiAncientWeightsModifier))
        {
            return null;
        }

        return runState.Modifiers.OfType<KoishiAncientWeightsModifier>().FirstOrDefault();
    }

    private static bool ShouldShowKoishiAncientWeight()
    {
        return EnableAncients && EnableAncientWeights;
    }

    private void AddAncientProbabilityChart(Control optionContainer)
    {
        NConfigCollapsibleSection? section = optionContainer
            .GetChildren()
            .OfType<NConfigCollapsibleSection>()
            .FirstOrDefault(child => child.Name == "CollapsibleSection_AncientWeights");

        if (section == null)
        {
            return;
        }

        AddExternalAncientWeightRows(section);

        _ancientProbabilityChart = new AncientProbabilityChart
        {
            Visible = EnableAncientWeights
        };

        _baseAncientWeightsSeparator = new HSeparator
        {
            Name = "KoishiBaseAncientWeightsSeparator",
            Visible = EnableAncientWeights && !EnableAncients,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };

        section.ContentContainer.AddChild(_baseAncientWeightsSeparator);
        section.ContentContainer.MoveChild(_baseAncientWeightsSeparator, 2);
        section.ContentContainer.AddChild(_ancientProbabilityChart);

        EventHandler refreshHandler = (_, _) =>
        {
            _baseAncientWeightsSeparator.Visible = EnableAncientWeights && !EnableAncients;
            _ancientProbabilityChart.Visible = EnableAncientWeights;
            _ancientProbabilityChart.Refresh();
        };
        ConfigChanged += refreshHandler;
        _configChangedHandlers.Add(refreshHandler);

        Action reloadHandler = () =>
        {
            _baseAncientWeightsSeparator.Visible = EnableAncientWeights && !EnableAncients;
            _ancientProbabilityChart.Visible = EnableAncientWeights;
            _ancientProbabilityChart.Refresh();
        };
        OnConfigReloaded += reloadHandler;
        _configReloadedHandlers.Add(reloadHandler);
    }

    private void AddExternalAncientWeightRows(NConfigCollapsibleSection section)
    {
        IReadOnlyList<AncientProbabilityInfo> externalAncients = AncientProbabilityData.ExternalInfos;
        if (externalAncients.Count == 0)
        {
            return;
        }

        Dictionary<string, int> weights = ParseExternalAncientWeights();
        foreach (AncientProbabilityInfo info in externalAncients)
        {
            try
            {
                weights.TryAdd(info.Key, DefaultAncientWeight);
                section.ContentContainer.AddChild(CreateExternalAncientWeightRow(info, weights[info.Key]));
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[KoishiAncientWeights] Failed to add external ancient row {info.Key}: {e}");
            }
        }

        ExternalAncientWeights = SerializeExternalAncientWeights(weights);
    }

    private Control CreateExternalAncientWeightRow(AncientProbabilityInfo info, int currentWeight)
    {
        Control label = CreateRawLabelControl($"{AncientProbabilityChart.LocalizedAncientName(info)} Weight", 28);
        NExternalAncientWeightSlider slider = new(info.Key, currentWeight, SetExternalAncientWeight);
        NConfigOptionRow row = new(ModPrefix, "ExternalAncientWeight_" + AncientProbabilityData.ConfigRowKey(info.Key), label, slider)
        {
            Visible = EnableAncientWeights
        };

        EventHandler refreshHandler = (_, _) => row.Visible = EnableAncientWeights;
        ConfigChanged += refreshHandler;
        _configChangedHandlers.Add(refreshHandler);

        Action reloadHandler = () =>
        {
            row.Visible = EnableAncientWeights;
            slider.SetWeight(GetExternalAncientWeight(info.Key));
        };
        OnConfigReloaded += reloadHandler;
        _configReloadedHandlers.Add(reloadHandler);

        return row;
    }

    private void SetExternalAncientWeight(string key, int weight)
    {
        Dictionary<string, int> weights = ParseExternalAncientWeights();
        weights[key] = int.Clamp(weight, 0, 10);
        ExternalAncientWeights = SerializeExternalAncientWeights(weights);
        Changed();
    }

    public static int GetExternalAncientWeight(string key)
    {
        Dictionary<string, int> weights = ParseExternalAncientWeights();
        return weights.TryGetValue(key, out int weight) ? int.Clamp(weight, 0, 10) : DefaultAncientWeight;
    }

    public static Dictionary<string, int> ParseExternalAncientWeights()
    {
        Dictionary<string, int> weights = new();
        foreach (string entry in ExternalAncientWeights.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string[] parts = entry.Split('=', 2, StringSplitOptions.TrimEntries);
            if (parts.Length == 2 && int.TryParse(parts[1], out int weight))
            {
                weights[parts[0]] = int.Clamp(weight, 0, 10);
            }
        }

        return weights;
    }

    private static Dictionary<string, int> CurrentExternalAncientWeights()
    {
        Dictionary<string, int> weights = ParseExternalAncientWeights();
        foreach (AncientProbabilityInfo info in AncientProbabilityData.ExternalInfos)
        {
            weights.TryAdd(info.Key, DefaultAncientWeight);
        }

        return weights;
    }

    public static string SerializeExternalAncientWeights(IReadOnlyDictionary<string, int> weights)
    {
        return string.Join(";", weights.OrderBy(pair => pair.Key).Select(pair => $"{pair.Key}={int.Clamp(pair.Value, 0, 10)}"));
    }
}

public readonly record struct AncientWeights(
    int MoriyaTwoGods,
    int HakureiReimu,
    int Orobas,
    int Pael,
    int Tezcatara,
    int Nonupeipe,
    int Tanx,
    int Vakuu,
    int Darv)
{
    public static AncientWeights Default { get; } = new(3, 3, 3, 3, 3, 3, 3, 3, 3);
}
