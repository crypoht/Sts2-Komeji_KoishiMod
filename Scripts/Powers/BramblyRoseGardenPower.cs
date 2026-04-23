using System;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players; 
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Pools;
using System.Collections.Generic;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Localization;
using KomeijiKoishi.Enums; 
using MegaCrit.Sts2.Core.Combat;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards; 
using MegaCrit.Sts2.Core.Models.Cards; 
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace KomeijiKoishi.Powers
{
    public sealed class BramblyRoseGardenPower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        
        // 设置为 Counter，支持层数堆叠
        public override PowerStackType StackType => PowerStackType.Counter;
        
        public override string? CustomPackedIconPath => $"res://mods/Komeiji_Koishi/images/powers/BramblyRoseGardenPower.png";
        public override string? CustomBigIconPath => $"res://mods/Komeiji_Koishi/images/powers/BramblyRoseGardenPower.png";

        // 🌟 定义动态变量 Percent，用于在本地化文本中显示 (层数 * 100)
        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new DynamicVar("Percent", base.Amount * 100m)
        };

        // 🌟 回合开始逻辑
        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            // 只有在持有者的回合开始时触发
            if (player == base.Owner.Player)
            {
                // 获取身上现有的荆棘 (ThornsPower)
                var thornsPower = base.Owner.Powers.FirstOrDefault(p => p is ThornsPower);
                
                if (thornsPower != null && thornsPower.Amount > 0)
                {
                    base.Flash();
                    
                    // 逻辑：增加 (当前荆棘 * 能力层数)
                    // 1层能力 = 增加 100%（翻倍）
                    // 2层能力 = 增加 200%（三倍）
                    decimal amountToAdd = thornsPower.Amount * base.Amount;
                    
                    await PowerCmd.Apply<ThornsPower>(base.Owner, amountToAdd, base.Owner, null, false);
                }
            }
        }
    }
}