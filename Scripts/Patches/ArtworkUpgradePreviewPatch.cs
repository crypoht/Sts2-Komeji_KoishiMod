using Godot;
using HarmonyLib;
using KomeijiKoishi.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;

namespace KomeijiKoishi.Patches
{
    [HarmonyPatch(typeof(NUpgradePreview), "Reload")]
    public static class ArtworkUpgradePreviewPatch
    {
        private static readonly AccessTools.FieldRef<NUpgradePreview, Control> AfterRef =
            AccessTools.FieldRefAccess<NUpgradePreview, Control>("_after");

        public static void Postfix(NUpgradePreview __instance)
        {
            CardModel? card = __instance.Card;
            if (card is not IArtworkEvolutionCard artwork || card.Pile == null)
            {
                return;
            }

            if (card.CurrentUpgradeLevel + 1 < card.MaxUpgradeLevel)
            {
                return;
            }

            CardModel nextCard = artwork.CreateEvolution();
            Control after = AfterRef(__instance);
            foreach (Node child in after.GetChildren(false))
            {
                child.QueueFreeSafely();
            }

            NCard? previewCard = NCard.Create(nextCard, ModelVisibility.NotSeen);
            if (previewCard == null)
            {
                return;
            }

            NPreviewCardHolder? holder = NPreviewCardHolder.Create(previewCard, true, false);
            if (holder == null)
            {
                return;
            }

            holder.FocusMode = Control.FocusModeEnum.None;
            after.AddChildSafely(holder);
            NCard? cardNode = holder.CardNode;
            cardNode?.UpdateVisuals(card.Pile.Type, CardPreviewMode.Normal);
        }
    }
}
