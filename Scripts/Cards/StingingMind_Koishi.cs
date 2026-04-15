using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Utils_Koishi;
using KomeijiKoishi.Enums;
using MegaCrit.Sts2.Core.HoverTips;


namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class StingingMind_Koishi : CustomCardModel
    {
        public StingingMind_Koishi() 
            : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromKeyword(KoishiKeywords.Unconscious) 
        };
        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DamageVar(5m, ValueProp.Move) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try
            {
                var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
                if (player == null || cardPlay.Target == null) return;

                await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                    .FromCard(this)
                    .Targeting(cardPlay.Target)
                    .WithHitFx("vfx/vfx_attack_slash") 
                    .Execute(choiceContext);


                var handPile = PileType.Hand.GetPile(player);
                
                var unconsciousCardsInHand = handPile.Cards.Where(c => 
                    KoishiExtensions.IsTrulyUnconscious(c) && 
                    (c.Keywords == null || !c.Keywords.Contains(CardKeyword.Unplayable)) &&
                    c != this 
                ).ToList();

                if (unconsciousCardsInHand.Count > 0)
                {
                    var targetCard = player.RunState.Rng.Shuffle.NextItem(unconsciousCardsInHand);

                    if (targetCard != null)
                    {
                        var validEnemies = base.CombatState!.HittableEnemies.Where(e => !e.IsDead).ToList();
                        Creature? autoTarget = validEnemies.Count > 0 ? player.RunState.Rng.Shuffle.NextItem(validEnemies) : null;

                        KoishiExtensions.AutoPlayedByUnconsciousCards.Add(targetCard);
                        
                        await CardCmd.AutoPlay(choiceContext, targetCard, autoTarget, AutoPlayType.Default, true, false);
                        
                        KoishiExtensions.AutoPlayedByUnconsciousCards.Remove(targetCard);
                    }
                }
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[StingingMind_Koishi] Error: {e.Message}");
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Damage.UpgradeValueBy(3m);
        }
    }
}