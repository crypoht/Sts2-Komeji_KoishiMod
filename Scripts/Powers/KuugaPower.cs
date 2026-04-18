using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using KomeijiKoishi.Pools;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players; 
using MegaCrit.Sts2.Core.Entities.Powers; 
using MegaCrit.Sts2.Core.Models; 
using MegaCrit.Sts2.Core.Models.Cards; 
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace KomeijiKoishi.Powers
{
    public sealed class KuugaPower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        
        public override string? CustomPackedIconPath => $"res://mods/Komeiji_Koishi/images/powers/KuugaPower.png";
        public override string? CustomBigIconPath => $"res://mods/Komeiji_Koishi/images/powers/KuugaPower.png";

        public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? card)
        {
            if (base.Owner != dealer || card == null || card.Type != CardType.Attack)
            {
                return 0m;
            }
            
            try 
            {
                if (card.EnergyCost.GetWithModifiers(CostModifiers.All) == 0 && !card.EnergyCost.CostsX)
                {
                    return base.Amount;
                }
            }
            catch (Exception ex)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[KuugaPower] 计算费用错误: {ex.Message}");
            }
            
            return 0m;
        }
    }
}