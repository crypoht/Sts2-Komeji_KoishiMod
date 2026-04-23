using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using KomeijiKoishi.Utils_Koishi;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models;
using KomeijiKoishi.Enums;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class HeartwardStrike_Koishi : CustomCardModel
    {
        public HeartwardStrike_Koishi() 
            : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true) 
        { 
        }

        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { KoishiKeywords.Closed, KoishiKeywords.Unconscious };

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DamageVar(5m, ValueProp.Move)
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as Player;
            if (player == null) return;

            if (cardPlay.Target != null)
            {
                await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                    .FromCard(this)
                    .Targeting(cardPlay.Target)
                    .WithHitFx("vfx/vfx_attack_blunt", null, null)
                    .Execute(choiceContext);
            }


            await ClosedStancePower.EnterThisStance(choiceContext, player, this);

            CardPile discardPile = PileType.Discard.GetPile(player);
            
            var unconsciousCardsInDiscard = discardPile.Cards.Where(c => KoishiExtensions.IsTrulyUnconscious(c)).ToList();

            if (unconsciousCardsInDiscard.Any())
            {
                CardSelectorPrefs prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1);
                
                IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromSimpleGrid(choiceContext, unconsciousCardsInDiscard, player, prefs);
                

                CardModel? selectedCard = selectedCards.FirstOrDefault();

                if (selectedCard != null)
                {
                    if (selectedCard.Pile?.Type == PileType.Discard || selectedCard.Pile?.Type == PileType.Draw)
                    {
                        await CardPileCmd.Add(selectedCard, PileType.Draw, CardPilePosition.Top, null, false);
                    }
                }
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Damage.UpgradeValueBy(3m);
        }
    }
}