using System;
using System.Collections.Generic;
using System.Linq; 
using System.Threading.Tasks;
using BaseLib.Utils;
using BaseLib.Abstracts; 
using MegaCrit.Sts2.Core.Combat; 
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures; 
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps; 
using KomeijiKoishi.Enums;

namespace KomeijiKoishi.Powers
{
    public sealed class TracingPower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Debuff;
        
        public override PowerStackType StackType => PowerStackType.Counter;

        public override string? CustomPackedIconPath => $"res://mods/Komeiji_Koishi/images/powers/TracingPower.png";

        public override string? CustomBigIconPath => $"res://mods/Komeiji_Koishi/images/powers/TracingPower.png";

        public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
        {
            if (side == base.Owner.Side)
            {
                await PowerCmd.TickDownDuration(this);
            }
        }

        public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        {

            if (target == null || target != base.Owner)
            {
                return 1m;
            }

            if (cardSource?.Tags?.Contains(KoishiTags.Danmaku) == true)
            {
                if (dealer != null)
                {
                    int successionAmount = dealer.GetPowerAmount<MentalStellarSuccessionPower>();
                    if (successionAmount > 0)
                    {
                        return 1.5m + ((decimal)successionAmount / 100m);
                    }
                }
                return 1.5m; 
            }

            return 1m;
        }
    }
}