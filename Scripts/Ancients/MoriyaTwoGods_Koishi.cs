using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using KomeijiKoishi;
using KomeijiKoishi.Relics;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace KomeijiKoishi.Ancients
{
    public sealed class MoriyaTwoGods_Koishi : CustomAncientModel, IAncientDialogueIconProvider
    {
        private const string DialogueIconDirectory = "res://mods/Komeiji_Koishi/images/ancients/";
        private const string KanakoDialogueIconFile = "MoriyaTwoGods_Koishi_kanako_dialogue_icon.png";
        private const string KanakoDialogueIconOutlineFile = "MoriyaTwoGods_Koishi_kanako_dialogue_icon.png";
        private const string SuwakoDialogueIconFile = "MoriyaTwoGods_Koishi_suwako_dialogue_icon.png";
        private const string SuwakoDialogueIconOutlineFile = "MoriyaTwoGods_Koishi_suwako_dialogue_icon.png";
        private const string KanakoSpeaker = "Kanako";
        private const string SuwakoSpeaker = "Suwako";

        public override string? CustomScenePath => "res://mods/Komeiji_Koishi/scenes/ancients/MoriyaTwoGods_Koishi.tscn";

        public override string? CustomMapIconPath => "res://mods/Komeiji_Koishi/images/ancients/MoriyaTwoGods_Koishi_map_icon.png";

        public override string? CustomMapIconOutlinePath => "res://mods/Komeiji_Koishi/images/ancients/MoriyaTwoGods_Koishi_map_icon_outline.png";

        public override string? CustomRunHistoryIconPath => "res://mods/Komeiji_Koishi/images/ancients/MoriyaTwoGods_Koishi_run_history_icon.png";

        public override string? CustomRunHistoryIconOutlinePath => "res://mods/Komeiji_Koishi/images/ancients/MoriyaTwoGods_Koishi_run_history_icon_outline.png";

        public override Color ButtonColor => new(0.15f, 0.42f, 0.32f, 0.5f);

        public override Color DialogueColor => new(0.15f, 0.42f, 0.32f);

        public override bool IsValidForAct(ActModel act) => KoishiModConfig.ActiveRunAncientsEnabled && act.ActNumber() == 2;

        public Texture2D? GetDialogueIcon(AncientDialogueLine line)
        {
            string? path = this.GetDialogueIconPath(line);
            return path == null ? null : PreloadManager.Cache.GetCompressedTexture2D(path);
        }

        public Texture2D? GetDialogueIconOutline(AncientDialogueLine line)
        {
            string? path = this.GetDialogueIconOutlinePath(line);
            return path == null ? null : PreloadManager.Cache.GetCompressedTexture2D(path);
        }

        private string? GetDialogueIconPath(AncientDialogueLine line)
        {
            string? speaker = GetSpeakerKey(line);
            if (speaker == KanakoSpeaker)
            {
                return GetOptionalDialogueIconPath(KanakoDialogueIconFile);
            }

            if (speaker == SuwakoSpeaker)
            {
                return GetOptionalDialogueIconPath(SuwakoDialogueIconFile);
            }

            return null;
        }

        private string? GetDialogueIconOutlinePath(AncientDialogueLine line)
        {
            string? speaker = GetSpeakerKey(line);
            if (speaker == KanakoSpeaker)
            {
                return GetOptionalDialogueIconPath(KanakoDialogueIconOutlineFile);
            }

            if (speaker == SuwakoSpeaker)
            {
                return GetOptionalDialogueIconPath(SuwakoDialogueIconOutlineFile);
            }

            return null;
        }

        private static string? GetOptionalDialogueIconPath(string fileName)
        {
            string path = DialogueIconDirectory + fileName;
            return ResourceLoader.Exists(path) ? path : null;
        }

        private static string? GetSpeakerKey(AncientDialogueLine line)
        {
            LocString? lineText = line.LineText;
            if (lineText == null)
            {
                return null;
            }

            return LocString.GetIfExists(lineText.LocTable, lineText.LocEntryKey + ".speaker")?.GetRawText();
        }

        protected override OptionPools MakeOptionPools { get; } = new OptionPools(
            MakePool(
                AncientOption<SuwakosHat_Koishi>(),
                AncientOption<TinyWhiteSnake_Koishi>(),
                AncientOption<FrogCard_Koishi>()
            ),
            MakePool(
                AncientOption<Onbashira_Koishi>(),
                AncientOption<KanakosShimenawa_Koishi>(),
                AncientOption<NuclearManagementManual_Koishi>()
            ),
            MakePool(
                AncientOption<FrogClothes_Koishi>(),
                AncientOption<MountainGodsBlessing_Koishi>(),
                AncientOption<MiracleGohei_Koishi>()
            )
        );
    }
}
