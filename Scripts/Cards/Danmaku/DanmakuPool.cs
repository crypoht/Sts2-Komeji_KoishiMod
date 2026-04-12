using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models; 
using MegaCrit.Sts2.Core.Models.Cards;

namespace KomeijiKoishi.Cards.Danmaku
{
    public static class DanmakuPool
    {
        // 只管装载卡牌模板的池子
        public static List<CardModel> Pool = new List<CardModel>();

        public static void Register(CardModel card)
        {
            if (!Pool.Exists(c => c.GetType() == card.GetType()))
            {
                Pool.Add(card);
            }
        }
    }
}