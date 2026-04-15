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
        public static List<Func<Player, CombatState, CardModel>> Generators = new();

        public static void Register<T>() where T : CardModel, new()
        {
            Generators.Add((owner, combatState) => combatState.CreateCard<T>(owner));
        }

        public static async Task<CardModel?> CreateRandomDanmakuInHand(Player owner, CombatState combatState)
        {
            if (Generators.Count == 0 || combatState == null) return null;

            var generator = owner.RunState.Rng.CombatCardGeneration.NextItem(Generators);
            CardModel? generatedCard = generator?.Invoke(owner, combatState);

            if (generatedCard != null)
            {
                await CardPileCmd.AddGeneratedCardToCombat(generatedCard, PileType.Hand, true, CardPilePosition.Bottom);
            }
            return generatedCard;
        }
    }
}