using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils; 
using BaseLib.Abstracts; 
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps; 
using MegaCrit.Sts2.Core.GameActions.Multiplayer; 
using KomeijiKoishi.Pools; 
using KomeijiKoishi.Enums;
using KomeijiKoishi.Powers; 

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class HeartAnger_Koishi : CustomCardModel, IStanceListenerCard
    {
        public HeartAnger_Koishi() 
            : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { KoishiKeywords.Bloom, KoishiKeywords.Closed };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DamageVar(18m, ValueProp.Move) 
        };

        protected override bool ShouldGlowGoldInternal => base.Owner?.Creature?.Powers.Any(p => p is ClosedStancePower && p.Amount > 0) ?? false;

        public void OnStanceChanged(bool isClosedStance, bool isBloomStance)
        {
            if (isClosedStance)
            {
                base.EnergyCost.SetThisCombat(1, false); 
            }
            else
            {
                base.EnergyCost.SetThisCombat(2, false);
            }
        }

        public override Task AfterCardEnteredCombat(CardModel card)
        {
            if (card == this)
            {
                bool isClosed = base.Owner?.Creature?.Powers.Any(p => p is ClosedStancePower && p.Amount > 0) ?? false;
                bool isBloom = base.Owner?.Creature?.Powers.Any(p => p is BloomStancePower && p.Amount > 0) ?? false;
                OnStanceChanged(isClosed, isBloom);
            }
            return Task.CompletedTask;
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try
            {
                if (cardPlay.Target == null) return;
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

                await Cmd.Wait(0.2f, false);

                await BloomStancePower.EnterThisStance(choiceContext, player, this);

                this.OnStanceChanged(false, true);
            }
            catch (Exception ex)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[HeartAnger] Error: {ex.Message}");
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Damage.UpgradeValueBy(4m);
        }
    }
}