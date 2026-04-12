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
using KomeijiKoishi.Powers; // 确保引用了 TracingPower 所在的命名空间

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class InnerAim_Koishi : CustomCardModel
    {
        public InnerAim_Koishi() 
            // 1费，技能牌，普通，目标为单体敌人
            : base(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DynamicVar("Tracing", 2m) // 初始给予 2 层追踪
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try
            {
                var player = base.Owner as Player;
                if (player == null || cardPlay.Target == null) return;

                // 给予 2 (升级后 3) 层追踪 (TracingPower)
                await PowerCmd.Apply<TracingPower>(
                    cardPlay.Target, 
                    base.DynamicVars["Tracing"].BaseValue, 
                    player.Creature, 
                    this, 
                    false
                );
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[InnerAim_Koishi] Error: {e.Message}");
            }
        }

        protected override void OnUpgrade()
        {
            // 升级时追踪层数 +1
            base.DynamicVars["Tracing"].UpgradeValueBy(1m);
        }
    }
}