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
            public int cardsLeft = 4;
            public int pendingDraws = 0; 
        }

        protected override object InitInternalData() => new Data();
        public override int DisplayAmount => GetInternalData<Data>().cardsLeft;
        public override bool IsInstanced => true;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new DynamicVar("BaseCards", 4m)
        };

        public override async Task AfterCardGeneratedForCombat(CardModel card, bool addedByPlayer)
        {
            if (card.Owner == base.Owner.Player && addedByPlayer)
            {
                if (card.GetType().Name.Contains("Danmaku"))
                {
                    Data data = GetInternalData<Data>();
                    data.cardsLeft--;
                    base.InvokeDisplayAmountChanged();

                    if (data.cardsLeft <= 0)
                    {
                        data.cardsLeft = 4; // 瞬间重置防止变成负数
                        base.InvokeDisplayAmountChanged(); 

                        base.Flash();
                        
                        // 🌟 只记账，不抽牌。Amount 是能力层数
                        data.pendingDraws += (int)base.Amount; 
                    }
                }
            }
            await Task.CompletedTask;
        }

        // 🌟 2. 结算阶段：等所有的 AutoPlay 和循环都跑完了再抽牌
        public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            // 确保是自己打出的牌触发的后续结算
            if (cardPlay.Card.Owner == base.Owner.Player)
            {
                Data data = GetInternalData<Data>();
                if (data.pendingDraws > 0)
                {
                    int amountToDraw = data.pendingDraws;
                    data.pendingDraws = 0; // 先清空账本，防止并发

                    var player = base.Owner?.Player;
                    if (player != null)
                    {
                        // 🌟 此时抽牌极其安全，且带有真实的 choiceContext
                        await CardPileCmd.Draw(choiceContext, amountToDraw, player, false);
                    }
                }
            }
        }
    }
}