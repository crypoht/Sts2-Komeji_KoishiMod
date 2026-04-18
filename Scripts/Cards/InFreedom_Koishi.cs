using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Enums; 
using System;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Powers;
using MegaCrit.Sts2.Core.Models;
using KomeijiKoishi.Utils_Koishi;
using MegaCrit.Sts2.Core.HoverTips;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class InFreedom_Koishi : CustomCardModel
    {
        public InFreedom_Koishi() 
            : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
        {
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new CardsVar(2) 
        };

         protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromKeyword(KoishiKeywords.Unconscious) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
            if (player == null) return;

            int maxCards = base.DynamicVars.Cards.IntValue;

            CardSelectorPrefs selectorPrefs = new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 0, maxCards);
            
            IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromHandForDiscard(choiceContext, player, selectorPrefs, null, this);

            if (selectedCards != null)
            {
                foreach (CardModel card in selectedCards)
                {
                    if (card.Tags != null)
                    {
                        KoishiExtensions.ApplyUnconsciousToCard(card);
                    }
                }

                await CardCmd.Discard(choiceContext, selectedCards);
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Cards.UpgradeValueBy(1m);
        }
    }
}