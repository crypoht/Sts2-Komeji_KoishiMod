using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection; 
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models; 
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Models.CardPools;
using System;
using BaseLib.Utils;

namespace KomeijiKoishi.Cards.Fumo
{
    [Pool(typeof(TokenCardPool))]
    public sealed class YuyukoFumo_Koishi : CustomCardModel
    {
        public YuyukoFumo_Koishi() 
            : base(0, CardType.Skill, CardRarity.Token, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/fumo/{GetType().Name}.png";
        
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain, CardKeyword.Exhaust };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as Player;
            if (player == null) return;

            int maxSelect = PileType.Hand.GetPile(player).Cards.Count;
            
            if (maxSelect > 0)
            {

                CardSelectorPrefs prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 0, maxSelect);
                IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromHand(choiceContext, player, prefs, null, this);
                
                foreach (CardModel card in selectedCards)
                {
                    await CardCmd.Exhaust(choiceContext, card, false, false);
                }
            }
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(+385); 
        }
    }
}