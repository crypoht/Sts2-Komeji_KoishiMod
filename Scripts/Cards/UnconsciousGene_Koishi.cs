using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Utils_Koishi;
using KomeijiKoishi.Enums;
using BaseLib.Utils; 
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers; 
using KomeijiKoishi.Powers; 


namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class UnconsciousGene_Koishi : CustomCardModel
    {
        public UnconsciousGene_Koishi()
            : base(9, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, true)
        {
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { KoishiTags.Subconscious };

        private class ReductionVar : DynamicVar { public ReductionVar(decimal val) : base("Reduction", val) { } }

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new DamageVar(24m, ValueProp.Move), 
            new ReductionVar(2m)               
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try 
            {
                ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
                var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
                if (player == null) return;

                if (player.Character != null)
                {
                    await CreatureCmd.TriggerAnim(player.Creature, "Attack", player.Character.CastAnimDelay);
                }

                await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                        .FromCard(this)
                        .Targeting(cardPlay.Target)
                        .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_heavy.mp3") 
                        .Execute(choiceContext);

                var handPile = PileType.Hand.GetPile(player);
                if (handPile != null && handPile.Cards != null)
                {
                    foreach (var c in handPile.Cards)
                    {
                        if (c != this && !KoishiExtensions.IsTrulyUnconscious(c))
                        {
                            KoishiExtensions.ApplyUnconsciousToCard(c);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[UnconsciousGene] 报错: {ex}");
            }
        }


        public override Task AfterCardEnteredCombat(CardModel card)
        {
            if (card != this || base.IsClone || base.CombatState == null) return Task.CompletedTask;

            int reductionAmount = base.DynamicVars["Reduction"].IntValue;

            int playedCount = CombatManager.Instance.History.CardPlaysFinished.Count((CardPlayFinishedEntry e) =>
                e.CardPlay.Card.Owner == base.Owner &&
                KoishiExtensions.IsTrulyUnconscious(e.CardPlay.Card)
            );

            if (playedCount > 0)
            {
                base.EnergyCost.AddThisCombat(-(playedCount * reductionAmount), false);
            }
            return Task.CompletedTask;
        }

        public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
        {
            if (cardPlay.Card.Owner != base.Owner) return Task.CompletedTask;
            
            if (KoishiExtensions.IsTrulyUnconscious(cardPlay.Card))
            {
                int reductionAmount = base.DynamicVars["Reduction"].IntValue;
                base.EnergyCost.AddThisCombat(-reductionAmount, false);
            }
            return Task.CompletedTask;
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Damage.UpgradeValueBy(8m);     
            base.DynamicVars["Reduction"].UpgradeValueBy(1m); 
        }
    }
}