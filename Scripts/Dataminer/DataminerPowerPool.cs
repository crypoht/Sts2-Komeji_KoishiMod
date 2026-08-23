using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace KomeijiKoishi.Dataminer
{
    public static class DataminerPowerPool
    {
        private static List<PowerModel>? powers;

        public static string? SelectRandomPowerId(MegaCrit.Sts2.Core.Entities.Players.Player owner)
        {
            List<PowerModel> pool = GetPowers();
            if (pool.Count == 0)
            {
                return null;
            }

            PowerModel selected = owner.RunState.Rng.CombatCardGeneration.NextItem(pool)!;
            return selected.Id.Entry;
        }

        public static PowerModel? Resolve(string? powerId)
        {
            if (string.IsNullOrEmpty(powerId))
            {
                return null;
            }

            return GetPowers().FirstOrDefault(power => power.Id.Entry == powerId);
        }

        private static List<PowerModel> GetPowers()
        {
            if (powers != null)
            {
                return powers;
            }

            powers = ModelDb.AllPowers
                .Where(power => power.GetType().Namespace?.StartsWith("MegaCrit.Sts2.", StringComparison.Ordinal) == true)
                .OrderBy(power => power.Id.Entry, StringComparer.Ordinal)
                .ToList();
            return powers;
        }
    }
}
