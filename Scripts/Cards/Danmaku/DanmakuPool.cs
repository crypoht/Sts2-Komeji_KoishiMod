using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Cards;
using BaseLib.Abstracts; 
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models; 
using MegaCrit.Sts2.Core.Models.Powers; 
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.GameActions.Multiplayer; 
using KomeijiKoishi.Pools; 
using KomeijiKoishi.Enums;

namespace KomeijiKoishi.Cards.Danmaku
{
    public static class DanmakuPool
    {
        public static async Task<CardModel?> CreateRandomDanmakuInHand(Player owner, CombatState combatState)
        {
            if (combatState == null) return null;

            var generators = new List<Func<Player, CombatState, CardModel>>
            {
                (p, c) => c.CreateCard<ArrowDanmaku_Koishi>(p),
                (p, c) => c.CreateCard<HeartDanmaku_Koishi>(p),
                (p, c) => c.CreateCard<LargeOrbDanmaku_Koishi>(p),
                (p, c) => c.CreateCard<RiceDanmaku_Koishi>(p),
                (p, c) => c.CreateCard<SmallOrbDanmaku_Koishi>(p),
                (p, c) => c.CreateCard<SquareDanmaku_Koishi>(p),
                (p, c) => c.CreateCard<StarDanmaku_Koishi>(p),
                (p, c) => c.CreateCard<YinYangOrbDanmaku_Koishi>(p),

            };


            var generator = owner.RunState.Rng.CombatCardGeneration.NextItem(generators);
            CardModel? generatedCard = generator?.Invoke(owner, combatState);

   
            if (generatedCard != null)
            {
                await CardPileCmd.AddGeneratedCardToCombat(generatedCard, PileType.Hand, true, CardPilePosition.Bottom);
            }
            
            return generatedCard;
        }
    }
}