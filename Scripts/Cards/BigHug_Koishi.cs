using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Enums;
using KomeijiKoishi.Utils_Koishi;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class BigHug_Koishi : CustomCardModel
    {
        private int _currentReduction = 0;

        public BigHug_Koishi()
            : base(3, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
        {
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new List<IHoverTip>
        {
            HoverTipFactory.FromKeyword(KoishiKeywords.Unconscious)
        };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new DamageVar(27m, ValueProp.Move)
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
                
                await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                    .FromCard(this)
                    .Targeting(cardPlay.Target)
                    .WithHitFx("vfx/vfx_attack_blunt") 
                    .Execute(choiceContext);
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[BigHug_Koishi] OnPlay Error: {e.Message}");
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Damage.UpgradeValueBy(9m);
        }

        private CardModel? GetEffectiveLastPlayedCard()
        {
            if (base.CombatState == null) return null;
            
            var history = CombatManager.Instance.History.CardPlaysFinished
                .Where(e => e.CardPlay.Card.Owner == base.Owner && e.HappenedThisTurn(base.CombatState))
                .ToList();
            
            if (history.Count == 0) return null;
            
            var last = history.Last();
            
            if (!last.CardPlay.IsAutoPlay && history.Count >= 2)
            {
                var prev = history[history.Count - 2];
                if (prev.CardPlay.IsAutoPlay) return prev.CardPlay.Card;
            }
            
            return last.CardPlay.Card;
        }

        protected override bool ShouldGlowGoldInternal
        {
            get
            {
                var effectiveLast = GetEffectiveLastPlayedCard();
                return effectiveLast != null && KoishiExtensions.IsTrulyUnconscious(effectiveLast);
            }
        }

        public override Task AfterCardEnteredCombat(CardModel card)
        {
            if (card != this) return Task.CompletedTask;
            if (base.IsClone) return Task.CompletedTask;

            _currentReduction = 0; 
            
            UpdateCostReduction(GetEffectiveLastPlayedCard(), false);
            
            return Task.CompletedTask;
        }

        public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
        {
            if (cardPlay.Card.Owner != base.Owner) return Task.CompletedTask;

            var previousPlay = CombatManager.Instance.History.CardPlaysFinished
                .LastOrDefault(e => e.CardPlay.Card.Owner == base.Owner && e.CardPlay != cardPlay);

            bool isFirstPlayOfTurn = previousPlay == null || !previousPlay.HappenedThisTurn(base.CombatState);


            CardModel effectiveCard = cardPlay.Card;
            

            if (!cardPlay.IsAutoPlay && previousPlay != null && previousPlay.HappenedThisTurn(base.CombatState))
            {
                if (previousPlay.CardPlay.IsAutoPlay)
                {
                    effectiveCard = previousPlay.CardPlay.Card; 
                }
            }


            UpdateCostReduction(effectiveCard, isFirstPlayOfTurn);
            
            return Task.CompletedTask;
        }

        private void UpdateCostReduction(CardModel? lastPlayedCard, bool isFirstPlayOfTurn)
        {
            if (isFirstPlayOfTurn)
            {
                _currentReduction = 0;
            }

            bool shouldReduce = lastPlayedCard != null && KoishiExtensions.IsTrulyUnconscious(lastPlayedCard);
            
            int targetReduction = shouldReduce ? 1 : 0;
            int diff = targetReduction - _currentReduction;
            
            if (diff != 0)
            {
                base.EnergyCost.AddThisTurn(-diff, false);
                _currentReduction = targetReduction;
            }
        }
    }
}