using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers; 
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;           
using MegaCrit.Sts2.Core.Models.Cards;
using KomeijiKoishi.Utils_Koishi; 

namespace KomeijiKoishi.Powers
{
    public sealed class PerfectMindControlPower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        
        public override PowerStackType StackType => PowerStackType.Counter;

        public override string? CustomPackedIconPath => $"res://mods/Komeiji_Koishi/images/powers/PerfectMindControl.png";
        public override string? CustomBigIconPath => $"res://mods/Komeiji_Koishi/images/powers/PerfectMindControl.png";

        public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
        {
            var player = base.Owner.Player;


            if (player != null && card.Owner == player && base.Amount > 0)
            {

                if (KoishiExtensions.IsTrulyUnconscious(card))
                {
                    base.Flash();

                    if (base.Amount <= 1)
                    {

                        await PowerCmd.Remove(this);
                    }
                    else
                    {

                        await PowerCmd.Apply<PerfectMindControlPower>(
                            choiceContext,
                            player.Creature, 
                            -1m, 
                            player.Creature, 
                            null, 
                            true 
                        );
                    }

                    await KoishiExtensions.SafeAutoPlayCard(choiceContext, player, card);
                }
            }
        }
    }
}