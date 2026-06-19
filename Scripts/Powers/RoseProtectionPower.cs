using MegaCrit.Sts2.Core.Models.Powers;
using System;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi; 
using KomeijiKoishi.Enums;
using KomeijiKoishi.Cards.Danmaku; 
using MegaCrit.Sts2.Core.Entities.Powers; 
using MegaCrit.Sts2.Core.Models; 
using MegaCrit.Sts2.Core.Models.Cards; 
using MegaCrit.Sts2.Core.GameActions.Multiplayer; 

namespace KomeijiKoishi.Powers
{
    public sealed class RoseProtectionPower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        
        public override string? CustomPackedIconPath => $"res://mods/Komeiji_Koishi/images/powers/{GetType().Name}.png";
        public override string? CustomBigIconPath => $"res://mods/Komeiji_Koishi/images/powers/{GetType().Name}.png";
    }
}