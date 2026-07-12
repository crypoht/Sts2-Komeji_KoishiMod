using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using KomeijiKoishi.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiSharedRelicPool))]
    public sealed class RedTie_Koishi : CustomRelicModel
    {
        private const decimal KuugaAmount = 2m;

        private int _attacksPlayedThisTurn;

        public override RelicRarity Rarity => RelicRarity.Ancient;

        public override string PackedIconPath => "res://mods/Komeiji_Koishi/images/relics/RedTie_Koishi.png";
        protected override string PackedIconOutlinePath => "res://mods/Komeiji_Koishi/images/relics/RedTie_Koishi.png";
        protected override string BigIconPath => "res://mods/Komeiji_Koishi/images/relics/RedTie_Koishi.png";

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player != base.Owner || player.Creature.IsDead)
            {
                return;
            }

            _attacksPlayedThisTurn = 0;
            base.Flash();
            await PowerCmd.Apply<KuugaPower>(choiceContext, player.Creature, KuugaAmount, player.Creature, null, false);
        }

        public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
        {
            if (participants.Contains(base.Owner.Creature))
            {
                _attacksPlayedThisTurn = 0;
            }

            return Task.CompletedTask;
        }

        public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.IsAutoPlay || cardPlay.Card.Owner != base.Owner || cardPlay.Card.Type != CardType.Attack)
            {
                return Task.CompletedTask;
            }

            _attacksPlayedThisTurn++;
            return Task.CompletedTask;
        }

        public override Task AfterCombatEnd(CombatRoom _)
        {
            _attacksPlayedThisTurn = 0;
            return Task.CompletedTask;
        }

        public override bool TryModifyEnergyCostInCombatLate(CardModel card, decimal originalCost, out decimal modifiedCost)
        {
            modifiedCost = originalCost;
            if (!ShouldMakeFree(card))
            {
                return false;
            }

            modifiedCost = 0m;
            return true;
        }

        public override bool TryModifyStarCost(CardModel card, decimal originalCost, out decimal modifiedCost)
        {
            modifiedCost = originalCost;
            if (!ShouldMakeFree(card))
            {
                return false;
            }

            modifiedCost = 0m;
            return true;
        }

        private bool ShouldMakeFree(CardModel card)
        {
            if (card.Owner != base.Owner || card.Type != CardType.Attack)
            {
                return false;
            }

            var pileType = card.Pile?.Type;
            if (pileType != PileType.Hand && pileType != PileType.Play)
            {
                return false;
            }

            return _attacksPlayedThisTurn == 0;
        }
    }
}
