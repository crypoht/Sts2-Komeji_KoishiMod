using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Enums;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiRelicPool))]
    public sealed class ScarletRose_Koishi : CustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Rare;

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
        {
            HoverTipFactory.FromKeyword(KoishiKeywords.Unconscious),
            HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
        };

        public override string PackedIconPath => "res://mods/Komeiji_Koishi/images/relics/ScarletRose_Koishi_outline.png";
        protected override string PackedIconOutlinePath => "res://mods/Komeiji_Koishi/images/relics/ScarletRose_Koishi.png";
        protected override string BigIconPath => "res://mods/Komeiji_Koishi/images/relics/ScarletRose_Koishi.png";

        public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
        {
            if (!IsAffectedCard(card, keywords))
            {
                return false;
            }

            keywords.Remove(CardKeyword.Unplayable);
            keywords.Add(CardKeyword.Exhaust);
            return true;
        }

        public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
        {
            if (!IsAffectedCard(card) || card.EnergyCost.CostsX)
            {
                modifiedCost = originalCost;
                return false;
            }

            modifiedCost = 0m;
            return true;
        }

        public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (IsAffectedCard(cardPlay.Card))
            {
                base.Flash();
            }

            return Task.CompletedTask;
        }

        private bool IsAffectedCard(CardModel card, ISet<CardKeyword>? currentKeywords = null)
        {
            return card.Owner == base.Owner
                && (card.Type == CardType.Status || card.Type == CardType.Curse)
                && IsUnconsciousForRelic(card, currentKeywords);
        }

        private static bool IsUnconsciousForRelic(CardModel card, ISet<CardKeyword>? currentKeywords)
        {
            if (card.Tags != null && card.Tags.Contains(KoishiTags.Unconscious))
            {
                return true;
            }

            if (card.CanonicalKeywords != null && card.CanonicalKeywords.Contains(KoishiKeywords.Unconscious))
            {
                return true;
            }

            if (currentKeywords != null)
            {
                return currentKeywords.Contains(KoishiKeywords.Unconscious);
            }

            if (card.Keywords != null && card.Keywords.Contains(KoishiKeywords.Unconscious))
            {
                return true;
            }

            return card.Owner?.Creature != null
                && card.Tags != null
                && card.Tags.Contains(KoishiTags.Subconscious)
                && card.Owner.Creature.Powers.Any(power => power is FetusDreamPower);
        }
    }
}
