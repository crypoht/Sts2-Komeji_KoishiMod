using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Creatures;
using KomeijiKoishi.Cards.Danmaku; 
using MegaCrit.Sts2.Core.Nodes.CommonUi; 
using KomeijiKoishi.Enums;
using MegaCrit.Sts2.Core.ValueProps; 
using MegaCrit.Sts2.Core.Localization.DynamicVars; 
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.HoverTips;
using KomeijiKoishi.Pools;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Players; 

namespace KomeijiKoishi.Powers
{
    public sealed class SelflessLovePower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter; 

        public override string? CustomPackedIconPath => $"res://mods/Komeiji_Koishi/images/powers/SelflessLovePower.png";
        public override string? CustomBigIconPath => $"res://mods/Komeiji_Koishi/images/powers/SelflessLovePower.png";

        private class Data
        {
            public int cardsLeft = 6;
            public int pendingDraws = 0; 
        }

        protected override object InitInternalData() => new Data();
        public override int DisplayAmount => GetInternalData<Data>().cardsLeft;
        public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new DynamicVar("BaseCards", 6m)
        };

        public override async Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
        {
            if (card.Owner == base.Owner.Player && creator == base.Owner.Player)
            {
                if (card.GetType().Name.Contains("Danmaku"))
                {
                    Data data = GetInternalData<Data>();
                    data.cardsLeft--;
                    base.InvokeDisplayAmountChanged();

                    if (data.cardsLeft <= 0)
                    {
                        data.cardsLeft = 6; 
                        base.InvokeDisplayAmountChanged(); 

                        base.Flash();
                        
                        data.pendingDraws += (int)base.Amount; 
                    }
                }
            }
            await Task.CompletedTask;
        }

        public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Card.Owner == base.Owner.Player)
            {
                Data data = GetInternalData<Data>();
                if (data.pendingDraws > 0)
                {
                    int amountToDraw = data.pendingDraws;
                    data.pendingDraws = 0; 

                    var player = base.Owner?.Player;
                    if (player != null)
                    {
                        await CardPileCmd.Draw(choiceContext, amountToDraw, player, false);
                    }
                }
            }
        }
    }
}