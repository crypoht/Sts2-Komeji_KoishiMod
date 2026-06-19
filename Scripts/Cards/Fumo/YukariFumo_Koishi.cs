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
using MegaCrit.Sts2.Core.Localization.DynamicVars; // 🌟 必须引入动态变量

namespace KomeijiKoishi.Cards.Fumo
{
    [Pool(typeof(TokenCardPool))]
    public sealed class YukariFumo_Koishi : CustomCardModel
    {
        public YukariFumo_Koishi() 
            : base(0, CardType.Skill, CardRarity.Token, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/fumo/{GetType().Name}.png";
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DynamicVar("FreeSpace", 0m) 
        };

        protected override bool IsPlayable
        {
            get
            {
                var player = base.Owner as Player;
                if (player != null)
                {
                    int currentHandCount = PileType.Hand.GetPile(player).Cards.Count;
                    int freeSpace = Math.Max(0, 10 - currentHandCount);
                    
                    base.DynamicVars["FreeSpace"].BaseValue = freeSpace;
                }
                return base.IsPlayable; 
            }
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as Player;
            if (player == null) return;


            int maxSelect = (int)base.DynamicVars["FreeSpace"].BaseValue;
            

            var drawPileCards = PileType.Draw.GetPile(player).Cards.ToList();

            if (maxSelect > 0 && drawPileCards.Count > 0)
            {
                maxSelect = Math.Min(maxSelect, drawPileCards.Count);

                CardSelectorPrefs prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 0, maxSelect);
                IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromSimpleGrid(choiceContext, drawPileCards, player, prefs);
                
                if (selectedCards.Any())
                {
                    await CardPileCmd.Add(selectedCards, PileType.Hand, CardPilePosition.Bottom, null, false);
                }
            }
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(+385); 
        }
    }
}