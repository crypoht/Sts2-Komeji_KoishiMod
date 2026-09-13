using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Enums;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers; 
using MegaCrit.Sts2.Core.HoverTips;
using KomeijiKoishi.Powers;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class Stare_Koishi : CustomCardModel
    {
        public Stare_Koishi() 
            : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true) { }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DamageVar(KomeijiKoishi.Config.KoishiBalanceManager.Value(6m, 5m), ValueProp.Move),
            new CardsVar(KomeijiKoishi.Config.KoishiBalanceManager.IsEnabled ? 2 : 1),
            new PowerVar<VulnerablePower>(1m),
            new PowerVar<TracingPower>(2m)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
            KomeijiKoishi.Config.KoishiBalanceManager.IsEnabled
                ? new[] { HoverTipFactory.FromPower<TracingPower>() }
                : new[] { HoverTipFactory.FromPower<VulnerablePower>() };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try
            {
                var player = base.Owner as Player;
                if (player == null || cardPlay.Target == null) return;


                await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                    .FromCard(this, cardPlay)
                    .Targeting(cardPlay.Target)
                    .WithHitFx("vfx/vfx_attack_blunt") 
                    .Execute(choiceContext);

    
                if (KomeijiKoishi.Config.KoishiBalanceManager.IsEnabled)
                {
                    await PowerCmd.Apply<TracingPower>(
                        choiceContext,
                        cardPlay.Target,
                        base.DynamicVars["TracingPower"].BaseValue,
                        player.Creature,
                        this,
                        false);
                }
                else
                {
                    await PowerCmd.Apply<VulnerablePower>(
                        choiceContext,
                        cardPlay.Target,
                        base.DynamicVars.Vulnerable.BaseValue,
                        player.Creature,
                        this,
                        false);
                }


                await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, player, false);
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[Stare_Koishi] Error: {e.Message}");
            }
        }

        protected override void OnUpgrade()
        {
            if (KomeijiKoishi.Config.KoishiBalanceManager.IsEnabled)
            {
                base.DynamicVars.Damage.UpgradeValueBy(3m);
            }
            else
            {
                base.DynamicVars.Cards.UpgradeValueBy(1m);
                base.DynamicVars.Vulnerable.UpgradeValueBy(1m);
            }
        }
    }
}
