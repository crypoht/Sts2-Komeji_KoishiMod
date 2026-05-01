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

namespace KomeijiKoishi.Cards.Fumo
{
    public static class FumoPool
    {
        public static async Task<CardModel?> CreateRandomFumoInHand(Player owner, CombatState combatState)
        {
            if (combatState == null) return null;

            var generators = new List<Func<Player, CombatState, CardModel>>
            {
                (p, c) => c.CreateCard<CirnoFumo_Koishi>(p),
                (p, c) => c.CreateCard<KogasaFumo_Koishi>(p),
                (p, c) => c.CreateCard<MarisaFumo_Koishi>(p),
                (p, c) => c.CreateCard<OkinaFumo_Koishi>(p),
                (p, c) => c.CreateCard<ReimuFumo_Koishi>(p),
                (p, c) => c.CreateCard<ReisenFumo_Koishi>(p),
                (p, c) => c.CreateCard<TewiFumo_Koishi>(p),
                (p, c) => c.CreateCard<YukariFumo_Koishi>(p),
                (p, c) => c.CreateCard<YuukaFumo_Koishi>(p),
                (p, c) => c.CreateCard<FlandreFumo_Koishi>(p),
                (p, c) => c.CreateCard<LWKoishiFumo_Koishi>(p),
                (p, c) => c.CreateCard<ShionFumo_Koishi>(p),
                (p, c) => c.CreateCard<YoumuFumo_Koishi>(p),
                (p, c) => c.CreateCard<NueFumo_Koishi>(p),
                (p, c) => c.CreateCard<YuyukoFumo_Koishi>(p)

            };

            var generator = owner.RunState.Rng.CombatCardGeneration.NextItem(generators);
            CardModel? generatedCard = generator?.Invoke(owner, combatState);
   
            if (generatedCard != null)
            {
                await CardPileCmd.AddGeneratedCardToCombat(generatedCard, PileType.Hand, true, CardPilePosition.Bottom);
            }
            
            return generatedCard;
        }

        // 生成随机 Fumo 到消耗堆
        public static async Task<CardModel?> CreateRandomFumoInExhaust(Player owner, CombatState combatState)
        {
            if (combatState == null) return null;

            var generators = new List<Func<Player, CombatState, CardModel>>
            {
                (p, c) => c.CreateCard<CirnoFumo_Koishi>(p),
                (p, c) => c.CreateCard<KogasaFumo_Koishi>(p),
                (p, c) => c.CreateCard<MarisaFumo_Koishi>(p),
                (p, c) => c.CreateCard<OkinaFumo_Koishi>(p),
                (p, c) => c.CreateCard<ReimuFumo_Koishi>(p),
                (p, c) => c.CreateCard<ReisenFumo_Koishi>(p),
                (p, c) => c.CreateCard<TewiFumo_Koishi>(p),
                (p, c) => c.CreateCard<YukariFumo_Koishi>(p),
                (p, c) => c.CreateCard<YuukaFumo_Koishi>(p),
                (p, c) => c.CreateCard<YuyukoFumo_Koishi>(p)
            };

            var generator = owner.RunState.Rng.CombatCardGeneration.NextItem(generators);
            CardModel? generatedCard = generator?.Invoke(owner, combatState);
   
            if (generatedCard != null)
            {
                await CardPileCmd.AddGeneratedCardToCombat(generatedCard, PileType.Exhaust, true, CardPilePosition.Bottom);
            }
            
            return generatedCard;
        }
    }
}