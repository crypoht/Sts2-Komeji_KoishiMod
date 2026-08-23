using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Models;

namespace KomeijiKoishi.Dataminer
{
    public static class DataminerEnchantmentPool
    {
        private static List<EnchantmentModel>? enchantments;

        public static string? SelectRandomEnchantmentId(MegaCrit.Sts2.Core.Entities.Players.Player owner)
        {
            List<EnchantmentModel> pool = GetEnchantments();
            return pool.Count == 0 ? null : owner.RunState.Rng.CombatCardGeneration.NextItem(pool)?.Id.Entry;
        }

        public static EnchantmentModel? Resolve(string? id)
        {
            return string.IsNullOrEmpty(id) ? null : GetEnchantments().FirstOrDefault(e => e.Id.Entry == id);
        }

        private static List<EnchantmentModel> GetEnchantments()
        {
            if (enchantments != null)
            {
                return enchantments;
            }

            enchantments = ModelDb.DebugEnchantments
                .Where(e => e.GetType().Namespace?.StartsWith("MegaCrit.Sts2.", StringComparison.Ordinal) == true)
                .Where(e => !e.GetType().Namespace!.Contains("Mocks", StringComparison.Ordinal))
                .Where(e => !e.Id.Entry.Contains("deprecated", StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.Id.Entry, StringComparer.Ordinal)
                .ToList();
            return enchantments;
        }
    }
}
