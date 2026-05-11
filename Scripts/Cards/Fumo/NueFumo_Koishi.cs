using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards; 
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Models.CardPools;
using BaseLib.Utils;

namespace KomeijiKoishi.Cards.Fumo
{
    [Pool(typeof(TokenCardPool))]
    public sealed class NueFumo_Koishi : CustomCardModel
    {
        public NueFumo_Koishi() 
            : base(0, CardType.Skill, CardRarity.Token, TargetType.Self, true) 
        { 
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/fumo/{GetType().Name}.png";
        
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as Player;
            if (player == null) return;

            IEnumerable<CardModel> handCards = PileType.Hand.GetPile(player).Cards.Where(c => !c.EnergyCost.CostsX);

            foreach (CardModel cardModel in handCards)
            {
                if (cardModel.EnergyCost.GetWithModifiers(CostModifiers.None) >= 0)
                {
                    int randomCost;

                    if (base.IsUpgraded)
                    {
                        randomCost = player.RunState.Rng.CombatEnergyCosts.NextInt(515);
                    }
                    else
                    {
                        randomCost = player.RunState.Rng.CombatEnergyCosts.NextInt(4);
                    }

                    cardModel.EnergyCost.SetThisTurnOrUntilPlayed(randomCost, false);

                    var ncard = NCard.FindOnTable(cardModel, null);
                    if (ncard != null)
                    {
                        ncard.PlayRandomizeCostAnim();
                    }
                }
            }

            await Task.CompletedTask;
        }

        protected override void OnUpgrade()
        {
        }
    }
}