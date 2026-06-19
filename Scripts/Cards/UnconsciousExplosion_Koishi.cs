using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Utils_Koishi;
using KomeijiKoishi.Enums;
using MegaCrit.Sts2.Core.HoverTips;
using BaseLib.Utils;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class UnconsciousExplosion_Koishi : CustomCardModel
    {
        public UnconsciousExplosion_Koishi() 
            : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
        {
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { KoishiTags.Subconscious };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromKeyword(KoishiKeywords.Unconscious) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as Player;
            if (player == null) return;

    
            await CreatureCmd.TriggerAnim(player.Creature, "Buff", player.Character!.CastAnimDelay);

  
            var handPile = PileType.Hand.GetPile(player);
            if (handPile != null && handPile.Cards != null)
            {
                foreach (var card in handPile.Cards)
                {

                    if (card != this)
                    {
                        KoishiExtensions.ApplyUnconsciousToCard(card);
                    }
                }
            }
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(-1);
        }
    }
}