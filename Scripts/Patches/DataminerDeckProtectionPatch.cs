using System;
using System.Threading;
using HarmonyLib;
using KomeijiKoishi.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace KomeijiKoishi.Patches
{
    // Dataminer is a combat-only token. This also protects it from other mods or
    // events that add cards through the normal deck validation hook.
    [HarmonyPatch(typeof(Hook), nameof(Hook.ShouldAddToDeck))]
    public static class DataminerDeckProtectionPatch
    {
        private static readonly AsyncLocal<int> InternalGenerationDepth = new();

        public static IDisposable AllowInternalGeneration()
        {
            InternalGenerationDepth.Value++;
            return new GenerationScope();
        }

        [HarmonyPostfix]
        private static void Postfix(CardModel card, ref bool __result)
        {
            if (card is DataminerCard_Koishi && InternalGenerationDepth.Value == 0)
            {
                __result = false;
            }
        }

        private sealed class GenerationScope : IDisposable
        {
            public void Dispose()
            {
                InternalGenerationDepth.Value = Math.Max(0, InternalGenerationDepth.Value - 1);
            }
        }
    }
}
