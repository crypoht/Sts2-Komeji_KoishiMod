using System;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Cards;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Creatures;
using KomeijiKoishi.Cards.Danmaku; 
using MegaCrit.Sts2.Core.Nodes.CommonUi; 
using KomeijiKoishi.Enums;
using MegaCrit.Sts2.Core.ValueProps; 
using MegaCrit.Sts2.Core.Models; 
using KomeijiKoishi.Cards;

namespace KomeijiKoishi.Powers
{
    public sealed class SelflessLovePower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override string? CustomPackedIconPath => $"res://mods/Komeiji_Koishi/images/powers/{GetType().Name}.png";
        public override string? CustomBigIconPath => $"res://mods/Komeiji_Koishi/images/powers/{GetType().Name}.png";

        private int _counter = 0;
        private const int Threshold = 4;

        public static bool IsBatchGenerating = false;
        public static int PendingDraws = 0;

        public override async Task AfterCardGeneratedForCombat(CardModel card, bool addedByPlayer)
        {
            if (card.Owner == base.Owner.Player && addedByPlayer)
            {
                if (card.GetType().Name.Contains("Danmaku"))
                {
                    _counter++;
                    
                    if (_counter >= Threshold)
                    {
                        _counter = 0;
                        base.Flash();

                        if (IsBatchGenerating)
                        {
                            PendingDraws += (int)base.Amount;
                        }
                        else
                        {
                            await CardPileCmd.Draw(null!, (int)base.Amount, base.Owner.Player, false);
                        }
                    }
                }
            }
        }
    }
}