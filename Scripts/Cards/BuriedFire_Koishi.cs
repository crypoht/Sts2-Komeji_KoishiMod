using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers; 
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using KomeijiKoishi.Utils_Koishi; 
using KomeijiKoishi.Enums;
using KomeijiKoishi.Cards.Danmaku; 

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class BuriedFire_Koishi : CustomCardModel
    {
        public BuriedFire_Koishi() 
            : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromKeyword(KoishiKeywords.Danmaku),
            HoverTipFactory.FromPower<TracingPower>() 
        };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DamageVar(3m, ValueProp.Move), 
            new DynamicVar("Tracing", 1m)      
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try
            {
                var player = base.Owner as Player;
                if (player == null || cardPlay.Target == null) return;

                await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                    .FromCard(this)
                    .Targeting(cardPlay.Target)
                    .WithHitFx("vfx/vfx_attack_blunt")
                    .Execute(choiceContext);

                var discardPile = PileType.Discard.GetPile(player);
                if (discardPile != null && discardPile.Cards.Count > 0)
                {
                    var cardsToPlay = discardPile.Cards.ToList();
                    int playedCount = 0;

                    foreach (var targetCard in cardsToPlay)
                    {
                        if (base.CombatState == null || base.CombatState.HittableEnemies.All(e => e.IsDead)) break;

                        var validEnemies = base.CombatState.HittableEnemies.Where(e => !e.IsDead).ToList();
                        Creature? autoTarget = validEnemies.Count > 0 ? player.RunState.Rng.Shuffle.NextItem(validEnemies) : null;

                        if (autoTarget != null)
                        {
                            if (targetCard.Keywords == null || !targetCard.Keywords.Contains(CardKeyword.Exhaust))
                            {
                                targetCard.AddKeyword(CardKeyword.Exhaust);
                            }

                            await CardCmd.AutoPlay(choiceContext, targetCard, autoTarget, AutoPlayType.Default, true, false);
                            playedCount++;
                        }
                    }

                    if (playedCount > 0)
                    {
                        var remainingEnemies = base.CombatState?.HittableEnemies.Where(e => !e.IsDead).ToList();
                        if (remainingEnemies != null)
                        {
                            foreach (var enemy in remainingEnemies)
                            {
                                await PowerCmd.Apply<TracingPower>(
                                    enemy, 
                                    base.DynamicVars["Tracing"].BaseValue * playedCount, 
                                    player.Creature, 
                                    this, 
                                    false
                                );
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[BuriedFire_Koishi] Error: {e.Message}");
            }
        }

       protected override void OnUpgrade() => base.EnergyCost.UpgradeBy(-1);
    }
}