using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiRelicPool))]
    public sealed class KoishiRock : CustomRelicModel
    {
        private readonly Dictionary<Type, BuffRecord> _buffRecords = new();
        private int _highestUpdatesPending;

        public override RelicRarity Rarity => RelicRarity.Rare;

        public override string PackedIconPath => "res://mods/Komeiji_Koishi/images/relics/KoishiRock_outline.png";
        protected override string PackedIconOutlinePath => "res://mods/Komeiji_Koishi/images/relics/KoishiRock.png";
        protected override string BigIconPath => "res://mods/Komeiji_Koishi/images/relics/KoishiRock.png";

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

        private sealed class BuffRecord
        {
            public decimal RealAmount;
            public decimal HighestAmount;
        }
    }
}
