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
        

        public override PowerStackType StackType => PowerStackType.Counter;
        
        public override string? CustomPackedIconPath => $"res://mods/Komeiji_Koishi/images/powers/BramblyRoseGardenPower.png";
        public override string? CustomBigIconPath => $"res://mods/Komeiji_Koishi/images/powers/BramblyRoseGardenPower.png";

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {

            if (player == base.Owner.Player)
            {

                var thornsPower = base.Owner.Powers.FirstOrDefault(p => p is ThornsPower);
                
                if (thornsPower != null && thornsPower.Amount > 0)
                {
                    base.Flash();
                    
                    decimal amountToAdd = thornsPower.Amount * base.Amount;
                    
                    await PowerCmd.Apply<ThornsPower>(base.Owner, amountToAdd, base.Owner, null, false);
                }
            }
        }
    }
}