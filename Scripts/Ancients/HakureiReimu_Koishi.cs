using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using KomeijiKoishi;
using KomeijiKoishi.Relics;
using MegaCrit.Sts2.Core.Models;

namespace KomeijiKoishi.Ancients
{
    public sealed class HakureiReimu_Koishi : CustomAncientModel
    {
        public override string? CustomScenePath => "res://mods/Komeiji_Koishi/scenes/ancients/HakureiReimu_Koishi.tscn";

        public override string? CustomMapIconPath => "res://mods/Komeiji_Koishi/images/ancients/HakureiReimu_Koishi_map_icon.png";

        public override string? CustomMapIconOutlinePath => "res://mods/Komeiji_Koishi/images/ancients/HakureiReimu_Koishi_map_icon.png";

        public override string? CustomRunHistoryIconPath => "res://mods/Komeiji_Koishi/images/ancients/HakureiReimu_Koishi_run_history_icon.png";

        public override string? CustomRunHistoryIconOutlinePath => "res://mods/Komeiji_Koishi/images/ancients/HakureiReimu_Koishi_run_history_icon.png";

        public override Color ButtonColor => new(0.72f, 0.12f, 0.12f, 0.5f);

        public override Color DialogueColor => new(0.72f, 0.12f, 0.12f);

        public override bool IsValidForAct(ActModel act) => KoishiModConfig.ActiveRunAncientsEnabled && act.ActNumber() == 3;

        protected override OptionPools MakeOptionPools { get; } = new OptionPools(
            MakePool(
                AncientOption<MikosBow_Koishi>(),
                AncientOption<MikoBlessing_Koishi>(),
                AncientOption<HakureiTorii_Koishi>()
            ),
            MakePool(
                AncientOption<YinYangGhostGodOrbRelic_Koishi>(),
                AncientOption<MiniSaisenBox_Koishi>(),
                AncientOption<RedTie_Koishi>(),
                AncientOption<ExorcismAmulet_Koishi>()
            ),
            MakePool(
                AncientOption<FreeGohei_Koishi>(),
                AncientOption<FadedMikoClothes_Koishi>(),
                AncientOption<KomainuStoneCarving_Koishi>()
            )
        );
    }
}
