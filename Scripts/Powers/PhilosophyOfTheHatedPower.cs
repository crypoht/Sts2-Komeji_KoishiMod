using System;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;

namespace KomeijiKoishi.Powers
{
    public sealed class PhilosophyOfTheHatedPower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        
        public override PowerStackType StackType => PowerStackType.Single;

        public override string? CustomPackedIconPath => $"res://mods/Komeiji_Koishi/images/powers/PhilosophyOfTheHatedPower.png";
        public override string? CustomBigIconPath => $"res://mods/Komeiji_Koishi/images/powers/PhilosophyOfTheHatedPower.png";

    }
}
