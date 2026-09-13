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
using MegaCrit.Sts2.Core.Combat;
using KomeijiKoishi.Utils_Koishi;
using KomeijiKoishi.Vfx;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class GiftYouFumo_Koishi : CustomCardModel
    {
        public GiftYouFumo_Koishi() 
            : base(0, CardType.Skill, CardRarity.Rare, TargetType.AnyAlly, true) 
        { 
        }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());
        
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust, KoishiKeywords.Fumo };

        public override CardMultiplayerConstraint MultiplayerConstraint
        {
            get { return CardMultiplayerConstraint.MultiplayerOnly; }
        }


        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var combatState = base.CombatState as MegaCrit.Sts2.Core.Combat.CombatState;

            if (cardPlay.Target?.Player == null || combatState == null) return;
            
            Player targetAlly = cardPlay.Target.Player;

            await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

            CardModel? fumoCard = await FumoPool.CreateRandomFumoInHand(targetAlly, combatState);

            if (fumoCard != null)
            {
                NGiftYouFumoVfx? vfx = NGiftYouFumoVfx.Create(base.Owner.Creature, targetAlly.Creature, fumoCard);
                if (vfx != null)
                {
                    NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx);
                }

                if (base.IsUpgraded)
                {
                    CardCmd.Upgrade(fumoCard, CardPreviewStyle.HorizontalLayout);
                }

            }
        }

        protected override void OnUpgrade()
        {
        }
    }
}
