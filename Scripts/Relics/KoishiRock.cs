using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using KomeijiKoishi.Cards.Danmaku;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiSharedRelicPool))]
    public sealed class KoishiRock : KoishiCollaborationRelic
    {
        private Dictionary<Type, BuffRecord> _buffRecords = new();
        private int _highestUpdatesPending;

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
            [HoverTipFactory.FromCard<ConsciousOverflow_Koishi>()];

        public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
        {
            if (player != Owner || player.Creature.IsDead || CombatManager.Instance.IsOverOrEnding)
                return;

            Flash();
            await CardPileCmd.AddGeneratedCardsToCombat(
                [combatState.CreateCard<ConsciousOverflow_Koishi>(player), combatState.CreateCard<ConsciousOverflow_Koishi>(player)],
                PileType.Draw, player, CardPilePosition.Random);
        }

        public override Task BeforeCombatStart()
        {
            _buffRecords.Clear();
            _highestUpdatesPending = 0;
            return Task.CompletedTask;
        }

        public override bool TryModifyPowerAmountReceived(
            PowerModel canonicalPower,
            Creature target,
            decimal amount,
            Creature? applier,
            out decimal modifiedAmount)
        {
            modifiedAmount = amount;

            if (!CombatManager.Instance.IsInProgress
                || target != base.Owner.Creature
                || canonicalPower.Type != PowerType.Buff
                || canonicalPower is KoishiStancePower
                || canonicalPower is MissMarysPhoneThisSidePower)
            {
                return false;
            }

            Type powerType = canonicalPower.GetType();
            decimal displayedBefore = GetDisplayedAmount(target, powerType);
            BuffRecord record = GetOrCreateRecord(powerType, displayedBefore);

            record.RealAmount += amount;
            if (record.RealAmount > record.HighestAmount)
            {
                record.HighestAmount = record.RealAmount;
                _highestUpdatesPending++;
            }

            decimal displayedAfter = Math.Max(record.RealAmount, record.HighestAmount);
            modifiedAmount = displayedAfter - displayedBefore;

            return modifiedAmount != amount || _highestUpdatesPending > 0;
        }

        public override Task AfterModifyingPowerAmountReceived(PowerModel power)
        {
            if (_highestUpdatesPending > 0 && power.Owner == base.Owner.Creature && power.Type == PowerType.Buff)
            {
                _highestUpdatesPending--;
                base.Flash();
            }

            return Task.CompletedTask;
        }

        public override Task AfterCombatEnd(CombatRoom _)
        {
            _buffRecords.Clear();
            _highestUpdatesPending = 0;
            return Task.CompletedTask;
        }

        private BuffRecord GetOrCreateRecord(Type powerType, decimal displayedAmount)
        {
            if (!_buffRecords.TryGetValue(powerType, out BuffRecord? record))
            {
                record = new BuffRecord
                {
                    RealAmount = displayedAmount,
                    HighestAmount = Math.Max(displayedAmount, 0m)
                };
                _buffRecords[powerType] = record;
            }

            return record;
        }

        private static decimal GetDisplayedAmount(Creature target, Type powerType)
        {
            PowerModel? power = target.Powers.FirstOrDefault(p => p.GetType() == powerType);
            return power?.Amount ?? 0m;
        }

        protected override void DeepCloneFields()
        {
            base.DeepCloneFields();
            _buffRecords = new();
        }

        private sealed class BuffRecord
        {
            public decimal RealAmount;
            public decimal HighestAmount;
        }
    }
}
