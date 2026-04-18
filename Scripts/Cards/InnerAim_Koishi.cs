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
using MegaCrit.Sts2.Core.HoverTips; 

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class InnerAim_Koishi : CustomCardModel
    {
        public InnerAim_Koishi() 
            : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DynamicVar("Tracing", 4m),
            new PowerVar<KuugaPower>(2m)
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

                await PowerCmd.Apply<TracingPower>(
                    cardPlay.Target, 
                    base.DynamicVars["Tracing"].BaseValue, 
                    player.Creature, 
                    this, 
                    false
                );
                await PowerCmd.Apply<KuugaPower>(
                player.Creature, 
                base.DynamicVars["KuugaPower"].BaseValue, 
                player.Creature, 
                this, 
                false);
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[InnerAim_Koishi] Error: {e.Message}");
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars["Tracing"].UpgradeValueBy(1m);
            base.DynamicVars["KuugaPower"].UpgradeValueBy(1m);
        }
    }
}