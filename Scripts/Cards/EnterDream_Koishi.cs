using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils; 
using BaseLib.Abstracts; 
using MegaCrit.Sts2.Core.CardSelection; 
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer; 
using KomeijiKoishi.Pools; 
using KomeijiKoishi.Enums;
using KomeijiKoishi.Powers; 

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class EnterDream_Koishi : CustomCardModel
    {
        public EnterDream_Koishi() 
            : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Ethereal, KoishiKeywords.Closed };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
            if (player == null) return;

            await CreatureCmd.TriggerAnim(player.Creature, "Buff", player.Character!.CastAnimDelay);

            await ClosedStancePower.EnterThisStance(choiceContext, player, this);

            await Cmd.Wait(0.2f, false);

            CardPile discardPile = PileType.Discard.GetPile(base.Owner);

            if (discardPile.Cards.Count > 0)
            {
                CardSelectorPrefs cardSelectorPrefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1);
                
                IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromSimpleGrid(choiceContext, discardPile.Cards, base.Owner, cardSelectorPrefs);
                CardModel? cardToRetrieve = selectedCards.FirstOrDefault();
                
                if (cardToRetrieve != null)
                {
                    await CardPileCmd.Add(cardToRetrieve, PileType.Hand, CardPilePosition.Bottom, null, false);
                }
            }
        }

        protected override void OnUpgrade()
        {
            base.RemoveKeyword(CardKeyword.Ethereal);
        }
    }
}