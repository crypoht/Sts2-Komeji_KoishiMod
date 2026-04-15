using System;
using BaseLib.Abstracts; 
using MegaCrit.Sts2.Core.Models.Powers;
using KomeijiKoishi.Pools;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players; 
using MegaCrit.Sts2.Core.Entities.Powers; 
using MegaCrit.Sts2.Core.Models; 
using MegaCrit.Sts2.Core.Models.Cards; 
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace KomeijiKoishi.Powers
{
    public sealed class MentalStellarSuccessionPower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override string? CustomPackedIconPath => "res://mods/Komeiji_Koishi/images/powers/MentalStellarSuccessionPower.png";

        public override string? CustomBigIconPath => $"res://test/powers/{GetType().Name}.png";
    }
}