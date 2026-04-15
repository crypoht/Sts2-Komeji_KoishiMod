using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils; 
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Enums;
using System;
using KomeijiKoishi.Utils_Koishi;
using MegaCrit.Sts2.Core.HoverTips;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class IdLiberation_Koishi : CustomCardModel, ITranscendenceCard
    {
        public string Name => "本能「本我的解放」"; 
        public new string Description => "本能刺击的先古超越形态。";

        public IdLiberation_Koishi() 
            : base(2, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy, true)
        {
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { KoishiTags.Unconscious, CardTag.Strike };

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { KoishiKeywords.Unconscious };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DamageVar(3m, ValueProp.Move), 
            new RepeatVar(9)                   
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target == null) return;
            var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player; 
            if (player == null) return;

            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                .WithHitCount(base.DynamicVars.Repeat.IntValue) 
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .Execute(choiceContext);

            var hand = PileType.Hand.GetPile(player);
            if (hand != null && hand.Cards.Count > 0)
            {
                var cardsToPlay = hand.Cards.Where(c => 
                    KoishiExtensions.IsTrulyUnconscious(c) && 
                    (c.Keywords == null || !c.Keywords.Contains(CardKeyword.Unplayable)) && 
                    c != this
                ).ToList();

                foreach (var targetCard in cardsToPlay)
                {
                    if (base.CombatState == null || base.CombatState.HittableEnemies.All(e => e.IsDead)) break;

                    var validEnemies = base.CombatState.HittableEnemies.Where(e => !e.IsDead).ToList();
                    Creature? autoTarget = validEnemies.Count > 0 ? player.RunState.Rng.Shuffle.NextItem(validEnemies) : null;

                    KoishiExtensions.AutoPlayedByUnconsciousCards.Add(targetCard);
                    
                    await CardCmd.AutoPlay(choiceContext, targetCard, autoTarget, AutoPlayType.Default, true, false);
                    
                    KoishiExtensions.AutoPlayedByUnconsciousCards.Remove(targetCard);
                }
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Damage.UpgradeValueBy(1m);
            base.DynamicVars.Repeat.UpgradeValueBy(3m);
        }

        public void Activate()
        {
        }
    }
}