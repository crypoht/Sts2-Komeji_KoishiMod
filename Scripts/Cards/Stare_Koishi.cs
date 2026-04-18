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
using KomeijiKoishi.Powers; 
using KomeijiKoishi.Enums;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers; 
using MegaCrit.Sts2.Core.HoverTips;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class Stare_Koishi : CustomCardModel
    {
        public Stare_Koishi() 
            : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DamageVar(4m, ValueProp.Move),    
            new CardsVar(1),                       
            new DynamicVar("Tracing", 1m)         
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromPower<TracingPower>() 
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

                await PowerCmd.Apply<TracingPower>(
                    cardPlay.Target, 
                    base.DynamicVars["Tracing"].BaseValue, 
                    player.Creature, 
                    this, 
                    false
                );

                await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, player, false);
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[Stare_Koishi] Error: {e.Message}");
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Cards.UpgradeValueBy(1m);
        }
    }
}