using BaseLib.Config;
using BaseLib.Config.UI;
using Godot;
using KomeijiKoishi.Config;
using KomeijiKoishi.Modifiers;
using KomeijiKoishi.Multiplayer;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using System.Linq;
using System.Text.Json;

namespace KomeijiKoishi;

public enum KoishiCardArtMode
{
    Normal,
    Ancient,
    Fumo
}

[ConfigHoverTipsByDefault]
public sealed class KoishiModConfig : SimpleModConfig
{
    private const int DefaultAncientWeight = 3;
    private static bool _useFumoCardArt;
    private static bool _useAncientCardArt;

    [ConfigSection("AncientSettings")]
    public static bool EnableAncients { get; set; } = true;

    [ConfigSection("BalanceSettings")]
    public static bool EnableBalancePatch { get; set; } = false;

    [ConfigSection("VisualSettings")]
    public static KoishiCardArtMode CardArtMode
    {
        get
        {
            if (_useFumoCardArt)
            {
                return KoishiCardArtMode.Fumo;
            }

            return _useAncientCardArt ? KoishiCardArtMode.Ancient : KoishiCardArtMode.Normal;
        }
        set
        {
            _useFumoCardArt = value == KoishiCardArtMode.Fumo;
            _useAncientCardArt = value == KoishiCardArtMode.Ancient;
        }
    }

    [ConfigHideInUI]
    public static bool UseFumoCardArt
    {
        get => _useFumoCardArt;
        set
        {
            _useFumoCardArt = value;
            if (value)
            {
                _useAncientCardArt = false;
            }
        }
    }

    [ConfigSection("VisualSettings")]
    [ConfigHideInUI]
    public static bool UseAncientCardArt
    {
        get => _useAncientCardArt;
        set
        {
            _useAncientCardArt = value;
            if (value)
            {
                _useFumoCardArt = false;
            }
        }
    }

    public static bool PlayMoriyaDanceForAllPlayers { get; set; } = false;

    [ConfigSection("AncientWeights")]
    [ConfigHideInUI]
    public static bool EnableAncientWeights { get; set; } = false;

    [ConfigHideInUI]
    [ConfigSlider(0, 10, 1)]
    public static int MoriyaTwoGodsWeight { get; set; } = DefaultAncientWeight;

    [ConfigHideInUI]
    [ConfigSlider(0, 10, 1)]
    public static int HakureiReimuWeight { get; set; } = DefaultAncientWeight;

    [ConfigHideInUI]
    [ConfigSlider(0, 10, 1)]
    public static int OrobasWeight { get; set; } = DefaultAncientWeight;

    [ConfigHideInUI]
    [ConfigSlider(0, 10, 1)]
    public static int PaelWeight { get; set; } = DefaultAncientWeight;

    [ConfigHideInUI]
    [ConfigSlider(0, 10, 1)]
    public static int TezcataraWeight { get; set; } = DefaultAncientWeight;

    [ConfigHideInUI]
    [ConfigSlider(0, 10, 1)]
    public static int NonupeipeWeight { get; set; } = DefaultAncientWeight;

    [ConfigHideInUI]
    [ConfigSlider(0, 10, 1)]
    public static int TanxWeight { get; set; } = DefaultAncientWeight;

    [ConfigHideInUI]
    [ConfigSlider(0, 10, 1)]
    public static int VakuuWeight { get; set; } = DefaultAncientWeight;

    [ConfigHideInUI]
    [ConfigSlider(0, 10, 1)]
    public static int DarvWeight { get; set; } = DefaultAncientWeight;

    [ConfigHideInUI]
    public static string ExternalAncientWeights { get; set; } = "";

    [ConfigIgnore]
    public static bool ActiveRunAncientsEnabled { get; private set; } = true;

    [ConfigIgnore]
    public static bool ActiveRunBalancePatchEnabled { get; private set; }

    [ConfigIgnore]
    public static bool ActiveRunPlayMoriyaDanceForAllPlayers { get; private set; }

    [ConfigIgnore]
    public static bool ActiveRunAncientWeightsEnabled { get; private set; }

    [ConfigIgnore]
    public static AncientWeights ActiveRunAncientWeights { get; private set; } = AncientWeights.Default;

    [ConfigIgnore]
    public static IReadOnlyDictionary<string, int> ActiveRunExternalAncientWeights { get; private set; } = new Dictionary<string, int>();

    [ConfigIgnore]
    public static int ActiveRunCurrentActIndex { get; private set; }

    [ConfigIgnore]
    private static bool HasSyncedRunConfig { get; set; }

    public override void SetupConfigUI(Control optionContainer)
    {
        base.SetupConfigUI(optionContainer);
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
        HasSyncedRunConfig = false;
        ApplyRunConfigFromModifiers(modifiers);
    }

    public static void BeginRunWithSyncedHostModifiers(IReadOnlyList<ModifierModel> modifiers)
    {
        HasSyncedRunConfig = true;
        ApplyRunConfigFromModifiers(modifiers);
        ApplySyncedRunConfigToLocalConfig();
    }

    private static void ApplyRunConfigFromModifiers(IReadOnlyList<ModifierModel> modifiers)
    {
        ActiveRunAncientsEnabled = !modifiers.Any(m => m is DisableKoishiAncientsModifier);
        ActiveRunBalancePatchEnabled = EnableBalancePatch;
        KoishiBalanceManager.SetEnabledForRun(ActiveRunBalancePatchEnabled);
        ActiveRunPlayMoriyaDanceForAllPlayers = modifiers.Any(m => m is MoriyaDanceForAllPlayersModifier);
        ActiveRunAncientWeightsEnabled = !modifiers.Any(m => m is DisableKoishiAncientWeightsModifier)
            && modifiers.Any(m => m is KoishiAncientWeightsModifier);
        ActiveRunAncientWeights = !ActiveRunAncientWeightsEnabled
            ? AncientWeights.Default
            : modifiers.OfType<KoishiAncientWeightsModifier>().FirstOrDefault()?.ToAncientWeights() ?? AncientWeights.Default;
        ActiveRunExternalAncientWeights = !ActiveRunAncientWeightsEnabled
            ? new Dictionary<string, int>()
            : modifiers.OfType<KoishiAncientWeightsModifier>().FirstOrDefault()?.ExternalWeights ?? new Dictionary<string, int>();
        ActiveRunCurrentActIndex = 0;
    }

    public static void BeginRunWithHostConfig(RunState state)
    {
        if (!HasSyncedRunConfig)
        {
            ApplyRunConfigFromModifiers(state.Modifiers);
        }

        ActiveRunCurrentActIndex = state.CurrentActIndex;
    }

    public static KoishiRunConfigMessage CreateRunConfigMessage()
    {
        return new KoishiRunConfigMessage
        {
            AncientsEnabled = EnableAncients,
            BalancePatchEnabled = EnableBalancePatch,
            PlayMoriyaDanceForAllPlayers = PlayMoriyaDanceForAllPlayers,
            AncientWeightsEnabled = EnableAncientWeights,
            Weights = CurrentAncientWeights,
            ExternalWeights = SerializeExternalAncientWeights(CurrentExternalAncientWeights())
        };
    }

    public static void ApplySyncedRunConfig(KoishiRunConfigMessage message)
    {
        HasSyncedRunConfig = true;
        ActiveRunAncientsEnabled = message.AncientsEnabled;
        ActiveRunBalancePatchEnabled = message.BalancePatchEnabled;
        ActiveRunPlayMoriyaDanceForAllPlayers = message.PlayMoriyaDanceForAllPlayers;
        ActiveRunAncientWeightsEnabled = message.AncientWeightsEnabled;
        ActiveRunAncientWeights = ActiveRunAncientWeightsEnabled ? message.Weights : AncientWeights.Default;
        ActiveRunExternalAncientWeights = ActiveRunAncientWeightsEnabled
            ? ParseExternalAncientWeights(message.ExternalWeights)
            : new Dictionary<string, int>();
        ActiveRunCurrentActIndex = 0;
        ApplySyncedRunConfigToLocalConfig();
    }

    private static void ApplySyncedRunConfigToLocalConfig()
    {
        EnableAncients = ActiveRunAncientsEnabled;
        EnableBalancePatch = ActiveRunBalancePatchEnabled;
        KoishiBalanceManager.SetEnabledForRun(ActiveRunBalancePatchEnabled);
        PlayMoriyaDanceForAllPlayers = ActiveRunPlayMoriyaDanceForAllPlayers;
        EnableAncientWeights = ActiveRunAncientWeightsEnabled;
        MoriyaTwoGodsWeight = ActiveRunAncientWeights.MoriyaTwoGods;
        HakureiReimuWeight = ActiveRunAncientWeights.HakureiReimu;
        OrobasWeight = ActiveRunAncientWeights.Orobas;
        PaelWeight = ActiveRunAncientWeights.Pael;
        TezcataraWeight = ActiveRunAncientWeights.Tezcatara;
        NonupeipeWeight = ActiveRunAncientWeights.Nonupeipe;
        TanxWeight = ActiveRunAncientWeights.Tanx;
        VakuuWeight = ActiveRunAncientWeights.Vakuu;
        DarvWeight = ActiveRunAncientWeights.Darv;
        ExternalAncientWeights = SerializeExternalAncientWeights(ActiveRunExternalAncientWeights);
    }

    public static bool HasSyncedRunConfigForCurrentRun()
    {
        return HasSyncedRunConfig;
    }

    public static IReadOnlyList<ModifierModel> WithActiveRunAncientConfig(IReadOnlyList<ModifierModel> modifiers)
    {
        List<ModifierModel> result = modifiers
            .Where(m => m is not KoishiAncientWeightsModifier and not DisableKoishiAncientWeightsModifier and not MoriyaDanceForAllPlayersModifier)
            .ToList();

        if (ActiveRunPlayMoriyaDanceForAllPlayers)
        {
            result.Add(ModelDb.Modifier<MoriyaDanceForAllPlayersModifier>().ToMutable());
        }

        bool hasMarker = result.Any(m => m is DisableKoishiAncientsModifier);
        if (ActiveRunAncientsEnabled)
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

        if (ActiveRunAncientWeightsEnabled)
        {
            result.Add(CreateWeightsModifier(ActiveRunAncientWeights, ActiveRunExternalAncientWeights));
        }
        else if (!result.Any(m => m is DisableKoishiAncientWeightsModifier))
        {
            result.Add(ModelDb.Modifier<DisableKoishiAncientWeightsModifier>().ToMutable());
        }

        return result;
    }

    public static bool HasRunAncientWeights(IRunState? runState)
    {
        return GetRunAncientWeightsModifier(runState) != null;
    }

    public static bool AreAncientWeightsEnabledForRun(IRunState? runState)
    {
        if (runState == null)
        {
            return HasSyncedRunConfig
                ? ActiveRunAncientsEnabled && ActiveRunAncientWeightsEnabled
                : EnableAncients && EnableAncientWeights;
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
        List<ModifierModel> result = modifiers
            .Where(m => m is not KoishiAncientWeightsModifier and not DisableKoishiAncientWeightsModifier and not MoriyaDanceForAllPlayersModifier)
            .ToList();

        if (PlayMoriyaDanceForAllPlayers)
        {
            result.Add(ModelDb.Modifier<MoriyaDanceForAllPlayersModifier>().ToMutable());
        }

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
        return CreateWeightsModifier(CurrentAncientWeights, CurrentExternalAncientWeights());
    }

    private static KoishiAncientWeightsModifier CreateWeightsModifier(AncientWeights weights, IReadOnlyDictionary<string, int> externalWeights)
    {
        KoishiAncientWeightsModifier modifier = ModelDb.Modifier<KoishiAncientWeightsModifier>().ToMutable() as KoishiAncientWeightsModifier
            ?? new KoishiAncientWeightsModifier();
        modifier.Apply(weights, externalWeights);
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
    private static IReadOnlyDictionary<string, int> CurrentExternalAncientWeights()
    {
        return ParseExternalAncientWeights(ExternalAncientWeights);
    }

    public static int GetExternalAncientWeight(string key)
    {
        return CurrentExternalAncientWeights().TryGetValue(key, out int weight) ? int.Clamp(weight, 0, 10) : 0;
    }

    private static string SerializeExternalAncientWeights(IReadOnlyDictionary<string, int> weights)
    {
        return JsonSerializer.Serialize(weights.ToDictionary(pair => pair.Key, pair => int.Clamp(pair.Value, 0, 10)));
    }

    private static IReadOnlyDictionary<string, int> ParseExternalAncientWeights(string serializedWeights)
    {
        if (string.IsNullOrWhiteSpace(serializedWeights))
        {
            return new Dictionary<string, int>();
        }

        try
        {
            Dictionary<string, int>? weights = JsonSerializer.Deserialize<Dictionary<string, int>>(serializedWeights);
            return weights?.ToDictionary(pair => pair.Key, pair => int.Clamp(pair.Value, 0, 10)) ?? new Dictionary<string, int>();
        }
        catch
        {
            return new Dictionary<string, int>();
        }
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
