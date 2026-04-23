using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Pools;
using BaseLib.Utils;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class MalletLoveSign_Koishi : CustomCardModel
    {
        public MalletLoveSign_Koishi() 
            : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true) 
        { 
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DamageVar(1m, ValueProp.Move),
            // 🌟 修复 1：参考本能刺击，使用标准的 RepeatVar
            new RepeatVar(4),
            // 🌟 修复 2：补上之前缺失的能量变量，否则执行时会报错
            new EnergyVar(1) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as Player;
            if (player == null || cardPlay.Target == null) return;

            // 🌟 修改：使用官方标准的 Repeat 属性
            int hits = base.DynamicVars.Repeat.IntValue;
            int energyToGain = 0;

            for (int i = 0; i < hits; i++)
            {
                var attackCommand = await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                    .FromCard(this)
                    .Targeting(cardPlay.Target)
                    .WithHitFx("vfx/vfx_attack_blunt", null, null) 
                    .Execute(choiceContext);

                if (attackCommand.Results.Any(r => r.TotalDamage > 0))
                {
                    energyToGain++;
                }
            }

            if (energyToGain > 0)
            {
                // 🌟 此时读取 base.DynamicVars.Energy 才是安全的
                await PlayerCmd.GainEnergy(energyToGain, player);
            }
        }

        protected override void OnUpgrade()
        {
            // 🌟 修改：升级时对应的变量名也要改
            base.DynamicVars.Repeat.UpgradeValueBy(2m);
        }
    }
}