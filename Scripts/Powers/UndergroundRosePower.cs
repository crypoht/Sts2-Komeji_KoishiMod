using System;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Creatures;
using KomeijiKoishi.Cards.Danmaku; 
using MegaCrit.Sts2.Core.Nodes.CommonUi; 
using KomeijiKoishi.Enums;
using MegaCrit.Sts2.Core.ValueProps; 
using MegaCrit.Sts2.Core.Entities.Powers; 
using MegaCrit.Sts2.Core.Models; 
using KomeijiKoishi.Cards;


namespace KomeijiKoishi.Powers
{
    public sealed class UndergroundRosePower : CustomPowerModel, IStanceListenerPower
    {
        public override PowerType Type => PowerType.Buff;
        
        public override PowerStackType StackType => PowerStackType.Counter;

        public override string? CustomPackedIconPath => $"res://mods/Komeiji_Koishi/images/powers/UndergroundRosePower.png";
        public override string? CustomBigIconPath => $"res://mods/Komeiji_Koishi/images/powers/UndergroundRosePower.png";

        public async Task OnStanceChanged(bool isClosedStance, bool isBloomStance, PlayerChoiceContext context, CardModel? sourceCard)
        {
            if ((isClosedStance || isBloomStance) && base.Owner?.Player != null && base.Owner.CombatState != null)
            {
                this.Flash();
                
                int playCount = (int)base.Amount;
                for (int i = 0; i < playCount; i++)
                {
                    var rose = base.Owner.CombatState.CreateCard<RoseDanmaku_Koishi>(base.Owner.Player);
                    

                    await CardCmd.AutoPlay(context, rose, null, AutoPlayType.Default, true, false);
                    
                    if (i < playCount - 1)
                    {
                        await Cmd.Wait(0.1f, false);
                    }
                }
            }
        }
    }
}