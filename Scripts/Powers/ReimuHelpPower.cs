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

using MegaCrit.Sts2.Core.Entities.Powers; 
using MegaCrit.Sts2.Core.Models; 



namespace KomeijiKoishi.Powers
{
    public sealed class ReimuHelpPower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.None;

        public override string? CustomPackedIconPath => $"res://mods/Komeiji_Koishi/images/powers/ReimuHelpPower.png";
        
        public override string? CustomBigIconPath => $"res://mods/Komeiji_Koishi/images/powers/ReimuHelpPower.png";
        public override async Task AfterCardDrawnEarly(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
        {
            if (card.Owner.Creature == base.Owner)
            {
                if (card is YinYangOrbDanmaku_Koishi)
                {
                    this.Flash(); 
                    
                    await CardCmd.AutoPlay(choiceContext, card, null, AutoPlayType.Default, true, false);
                }
            }
        }
    }
}