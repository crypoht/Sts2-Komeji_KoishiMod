using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System;
using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi; 
using KomeijiKoishi.Enums;
using KomeijiKoishi.Cards.Danmaku; 
using MegaCrit.Sts2.Core.Entities.Powers; 
using MegaCrit.Sts2.Core.Models; 
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps; 

namespace KomeijiKoishi.Powers
{
    public sealed class SecondNeedPower : CustomPowerModel, IStanceListenerPower
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override string? CustomPackedIconPath => $"res://mods/Komeiji_Koishi/images/powers/SecondNeedPower.png";
        public override string? CustomBigIconPath => $"res://mods/Komeiji_Koishi/images/powers/SecondNeedPower.png";

        public async Task OnStanceChanged(bool isClosedStance, bool isBloomStance, PlayerChoiceContext context, CardModel? sourceCard)
        {
            try 
            {
                if (base.Owner == null || base.Amount <= 0) return;

                this.Flash(); 
                await CreatureCmd.GainBlock(base.Owner, base.Amount, ValueProp.Unpowered, null, false);
            }
            catch (System.Exception ex)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[SecondNeedPower] 叠甲报错: {ex.Message}");
            }
        }
    }
}