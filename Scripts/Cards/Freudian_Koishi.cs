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

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new BlockVar(12m, ValueProp.Move) 
        };

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
                return hand != null && hand.Cards.Any(c => c != this && KoishiExtensions.IsTrulyUnconscious(c));
            }
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as Player;
            if (player == null) return;

            await CreatureCmd.TriggerAnim(player.Creature, "Buff", player.Character!.CastAnimDelay);

            var handPile = PileType.Hand.GetPile(player);
            int count = handPile.Cards.Count(c => KoishiExtensions.IsTrulyUnconscious(c));

            if (count > 0)
            {
                decimal totalBlock = base.DynamicVars.Block.BaseValue * count;

                await CreatureCmd.GainBlock(player.Creature, totalBlock, ValueProp.Move, null, false);
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Block.UpgradeValueBy(8m);
        }
    }
}