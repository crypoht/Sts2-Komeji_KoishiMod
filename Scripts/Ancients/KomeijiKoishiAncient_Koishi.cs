using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Patches.Content;
using BaseLib.Utils;
using Godot;
using KomeijiKoishi.Relics;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;

namespace KomeijiKoishi.Ancients;

public sealed class KomeijiKoishiAncient_Koishi : CustomAncientModel, ICollaborationAncient
{
    private const string AssetRoot = "res://mods/Komeiji_Koishi/";
    private const string IconStem = AssetRoot + "images/ancients/KomeijiKoishiAncient_Koishi";
    private static readonly IReadOnlyList<string> RequiredCollaborationModIds =
        Array.AsReadOnly(new[] { "Komeiji_Koishi", "KazamiYuka" });

    IReadOnlyList<string> ICollaborationAncient.RequiredModIds => RequiredCollaborationModIds;

    public override string CustomScenePath => AssetRoot + "scenes/ancients/KomeijiKoishiAncient_Koishi.tscn";
    public override string CustomMapIconPath => IconStem + "_map_icon.png";
    public override string CustomMapIconOutlinePath => CustomMapIconPath;
    public override string CustomRunHistoryIconPath => IconStem + "_run_history_icon.png";
    public override string CustomRunHistoryIconOutlinePath => CustomRunHistoryIconPath;

    public override Color ButtonColor => new(0.22f, 0.34f, 0.61f, 0.5f);
    public override Color DialogueColor => new(0.22f, 0.34f, 0.61f);

    public override bool IsValidForAct(ActModel act) =>
        KoishiModConfig.AreSpecialAncientsEnabledForRun(CurrentGeneratingRunState.State)
        && act.ActNumber() == 3
        && ModManager.GetLoadedMods().Any(mod =>
            string.Equals(mod.manifest?.id, "KazamiYuka", StringComparison.OrdinalIgnoreCase));

    protected override OptionPools MakeOptionPools => new(
        MakePool(AncientOption<KoishiRock>(), AncientOption<ClosedEyePendant_Koishi>(), AncientOption<UnconsciousYoukaiMousse_Koishi>()),
        MakePool(AncientOption<KoishisTelephone_Koishi>(), AncientOption<HotSpringTicket_Koishi>(), AncientOption<FantasyFortuneSlip_Koishi>()),
        MakePool(AncientOption<DeepBlueRose_Koishi>(), AncientOption<UnconsciousKnifeRelic_Koishi>(), AncientOption<Dream_Koishi>()));
}
