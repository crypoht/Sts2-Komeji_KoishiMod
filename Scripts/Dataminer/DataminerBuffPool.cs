using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace KomeijiKoishi.Dataminer
{
    /// <summary>
    /// Official powers only. Both positive and negative powers are included.
    /// </summary>
    public static class DataminerBuffPool
    {
        private static List<PowerModel>? powers;

        public static string? SelectRandomBuffId(
            Player owner,
            bool allowEnemyTargets = true,
            bool allowNegativeAmounts = true)
        {
            List<PowerModel> pool = GetPowers()
                .Where(power => allowEnemyTargets || DataminerPowerPool.IsAllowedForEnemyTarget(power))
                .Where(power => allowNegativeAmounts || power.AllowNegative)
                .ToList();
            return pool.Count == 0
                ? null
                : owner.RunState.Rng.CombatCardGeneration.NextItem(pool)?.Id.Entry;
        }

        public static PowerModel? Resolve(string? powerId)
        {
            if (string.IsNullOrEmpty(powerId))
            {
                return null;
            }

            return GetPowers().FirstOrDefault(power => power.Id.Entry == powerId);
        }

        public static IReadOnlyList<PowerModel> GetOfficialDebuffs()
        {
            return GetPowers()
                .Where(power => power.TypeForCurrentAmount == PowerType.Debuff)
                .ToList();
        }

        private static List<PowerModel> GetPowers()
        {
            if (powers != null)
            {
                return powers;
            }

            powers = ModelDb.AllPowers
                .Where(DataminerPowerPool.IsAllowedOfficialPower)
                .OrderBy(power => power.Id.Entry, StringComparer.Ordinal)
                .ToList();
            return powers;
        }
    }
}
