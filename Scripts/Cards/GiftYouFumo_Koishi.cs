using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Models.CardPools;
using KomeijiKoishi.Cards.Fumo; 
using BaseLib.Utils;
using KomeijiKoishi.Enums;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class GiftYouFumo_Koishi : CustomCardModel
    {
        public GiftYouFumo_Koishi() 
            : base(0, CardType.Skill, CardRarity.Rare, TargetType.AnyAlly, true) 
        { 
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";
        
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust, KoishiKeywords.Fumo };

        public override CardMultiplayerConstraint MultiplayerConstraint
        {
            get { return CardMultiplayerConstraint.MultiplayerOnly; }
        }


        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target == null || cardPlay.Target.Player == null || base.CombatState == null) return;
            
            Player targetAlly = cardPlay.Target.Player;

            await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

            CardModel? fumoCard = await FumoPool.CreateRandomFumoInHand(targetAlly, base.CombatState);

            if (fumoCard != null)
            {
                if (base.IsUpgraded)
                {
                    CardCmd.Upgrade(fumoCard, CardPreviewStyle.HorizontalLayout);
                }

                if (fumoCard is NueFumo_Koishi && targetAlly != base.Owner)
                {
                    await CardCmd.AutoPlay(choiceContext, fumoCard, null, AutoPlayType.Default, true, false); //
                }
            }
        }

        protected override void OnUpgrade()
        {
        }
    }
}