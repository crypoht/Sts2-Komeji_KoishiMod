using Godot;
using HarmonyLib;
using KomeijiKoishi.Ancients;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Events;

namespace KomeijiKoishi.Patches
{
    [HarmonyPatch(typeof(NAncientDialogueLine), "SetAncientAsSpeaker")]
    public static class AncientDialogueIconPatch
    {
        private static readonly AccessTools.FieldRef<NAncientDialogueLine, AncientEventModel> AncientRef =
            AccessTools.FieldRefAccess<NAncientDialogueLine, AncientEventModel>("_ancient");

        private static readonly AccessTools.FieldRef<NAncientDialogueLine, AncientDialogueLine> LineRef =
            AccessTools.FieldRefAccess<NAncientDialogueLine, AncientDialogueLine>("_line");

        [HarmonyPostfix]
        public static void Postfix(NAncientDialogueLine __instance)
        {
            if (AncientRef(__instance) is not IAncientDialogueIconProvider iconProvider)
            {
                return;
            }

            AncientDialogueLine line = LineRef(__instance);
            Texture2D? icon = iconProvider.GetDialogueIcon(line);
            Texture2D? outline = iconProvider.GetDialogueIconOutline(line);
            if (icon == null && outline == null)
            {
                return;
            }

            Control ancientIcon = __instance.GetNode<Control>("%AncientIcon");
            if (icon != null)
            {
                ancientIcon.GetNode<TextureRect>("Icon").Texture = icon;
            }

            if (outline != null)
            {
                ancientIcon.GetNode<TextureRect>("Icon/Outline").Texture = outline;
            }
        }
    }
}
