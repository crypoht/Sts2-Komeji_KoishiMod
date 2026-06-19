using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using BaseLib.Abstracts; 
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures; 
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Enums;
using MegaCrit.Sts2.Core.Combat;

namespace KomeijiKoishi.Powers
{
    public sealed class FlawPower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Debuff;
        public override PowerStackType StackType => PowerStackType.Counter;
        
        public override string? CustomPackedIconPath => $"res://mods/Komeiji_Koishi/images/powers/FlawPower.png";
        public override string? CustomBigIconPath => $"res://mods/Komeiji_Koishi/images/powers/FlawPower.png";


        public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
        {
            if (side == base.Owner.Side)
            {
                base.Flash(); 
                await PowerCmd.Apply<KuugaPower>(new ThrowingPlayerChoiceContext(), base.Owner, -base.Amount, base.Owner, null, false);
            }

            await base.AfterSideTurnStart(side, participants, combatState);
        }
    }
}