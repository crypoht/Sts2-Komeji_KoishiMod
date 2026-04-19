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
using KomeijiKoishi.Enums;
using MegaCrit.Sts2.Core.ValueProps; 
using MegaCrit.Sts2.Core.Entities.Powers; 
using MegaCrit.Sts2.Core.Models; 
using KomeijiKoishi.Utils_Koishi; 

namespace KomeijiKoishi.Powers
{
    public sealed class PhilosophyOfTheHatedPower : CustomPowerModel, IStanceListenerPower
    {
        public override PowerType Type => PowerType.Buff;
        
        public override PowerStackType StackType => PowerStackType.Counter;

        public override string? CustomPackedIconPath => $"res://mods/Komeiji_Koishi/images/powers/PhilosophyOfTheHatedPower.png";
        public override string? CustomBigIconPath => $"res://mods/Komeiji_Koishi/images/powers/PhilosophyOfTheHatedPower.png";

        public async Task OnStanceChanged(bool isClosedStance, bool isBloomStance, PlayerChoiceContext context, CardModel? sourceCard)
        {
            if (isClosedStance && base.Owner?.Player != null)
            {
                var player = base.Owner.Player;
                var handPile = PileType.Hand.GetPile(player);

                if (handPile != null && handPile.Cards != null)
                {
                    this.Flash();

                    int playCount = (int)base.Amount;
                    for (int i = 0; i < playCount; i++)
                    {
                        var unconsciousCard = handPile.Cards.FirstOrDefault(c => KoishiExtensions.IsTrulyUnconscious(c));

                        if (unconsciousCard != null)
                        {
                            await CardCmd.AutoPlay(context, unconsciousCard, null, AutoPlayType.Default, true, false);
                        }
                        else
                        {
                            break; 
                        }
                    }
                }
            }
        }
    }
}