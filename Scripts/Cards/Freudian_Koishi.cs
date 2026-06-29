using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.HoverTips;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Utils_Koishi;
using KomeijiKoishi.Enums;
using BaseLib.Utils;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class Freudian_Koishi : CustomCardModel
    {
        public Freudian_Koishi() 
            : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self, true) 
        { 
        }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new BlockVar(11m, ValueProp.Move) 
        };

        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { KoishiTags.Unconscious};

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { KoishiKeywords.Unconscious };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromKeyword(KoishiKeywords.Unconscious) 
        };

        protected override bool ShouldGlowGoldInternal
        {
            get
            {
                var player = base.Owner as Player;
                if (player == null) return false;
                var hand = PileType.Hand.GetPile(player);
                return hand != null && CountUnconsciousCardsIncludingSelf(hand) > 0;
            }
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as Player;
            if (player == null) return;

            await CreatureCmd.TriggerAnim(player.Creature, "Buff", player.Character!.CastAnimDelay);

            var handPile = PileType.Hand.GetPile(player);
            int count = CountUnconsciousCardsIncludingSelf(handPile);

            if (count > 0)
            {
                decimal totalBlock = base.DynamicVars.Block.BaseValue * count;

                await CreatureCmd.GainBlock(player.Creature, totalBlock, ValueProp.Move, null, false);
            }
        }

        private int CountUnconsciousCardsIncludingSelf(CardPile handPile)
        {
            int count = handPile.Cards.Count(c => KoishiExtensions.IsTrulyUnconscious(c));

            if (!handPile.Cards.Contains(this) && KoishiExtensions.IsTrulyUnconscious(this))
            {
                count++;
            }

            return count;
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Block.UpgradeValueBy(6m);
        }
    }
}
