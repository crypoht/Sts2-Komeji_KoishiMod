using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Enums;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;          
using MegaCrit.Sts2.Core.Models.Cards;
using KomeijiKoishi.Utils_Koishi; 
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Entities.Creatures;


namespace KomeijiKoishi.Powers
{
    public sealed class LostInFlowerFieldPower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        
        public override PowerStackType StackType => PowerStackType.Counter;

        public override string? CustomPackedIconPath => $"res://mods/Komeiji_Koishi/images/powers/LostInFlowerFieldPower.png";
        public override string? CustomBigIconPath => $"res://mods/Komeiji_Koishi/images/powers/LostInFlowerFieldPower.png";

        public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
        {
            if (side == base.Owner.Side)
            {
                var player = base.Owner.Player;
                if (player == null) return;

                var exhaustPile = PileType.Exhaust.GetPile(player);
                if (exhaustPile == null) return;

                var validCards = exhaustPile.Cards.Where(c => c.Keywords != null && c.Keywords.Contains(KoishiKeywords.LostInFlowerField)).ToList();

                if (validCards.Count <= 0 || this.Amount <= 0)
                {
                    return;
                }

                base.Flash(); 

                int pullCount = Math.Min(this.Amount, validCards.Count);

                IEnumerable<CardModel> cardsToPull = validCards.TakeRandom(pullCount, player.RunState.Rng.CombatCardSelection);

                foreach (var card in cardsToPull)
                {
                    await CardPileCmd.Add(card, PileType.Hand, CardPilePosition.Bottom, null, false);
                }
            }
        }
    }
}
