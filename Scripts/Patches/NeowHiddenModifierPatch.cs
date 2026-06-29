using HarmonyLib;
using KomeijiKoishi.Modifiers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Runs;

namespace KomeijiKoishi.Patches
{
    [HarmonyPatch(typeof(Neow), "GenerateInitialOptions")]
    public static class NeowHiddenModifierPatch
    {
        private static readonly Action<RunState, IReadOnlyList<ModifierModel>> SetModifiers =
            (Action<RunState, IReadOnlyList<ModifierModel>>)Delegate.CreateDelegate(
                typeof(Action<RunState, IReadOnlyList<ModifierModel>>),
                AccessTools.PropertySetter(typeof(RunState), nameof(RunState.Modifiers)));

        public static void Prefix(Neow __instance, ref IReadOnlyList<ModifierModel>? __state)
        {
            if (__instance.Owner?.RunState is not RunState runState)
            {
                return;
            }

            IReadOnlyList<ModifierModel> modifiers = runState.Modifiers;
            List<ModifierModel> visibleModifiers = modifiers
                .Where(m => m is not KoishiAncientWeightsModifier and not DisableKoishiAncientWeightsModifier and not DisableKoishiAncientsModifier)
                .ToList();

            if (visibleModifiers.Count == modifiers.Count)
            {
                return;
            }

            __state = modifiers;
            SetModifiers(runState, visibleModifiers);
        }

        public static void Postfix(Neow __instance, IReadOnlyList<ModifierModel>? __state)
        {
            if (__state == null)
            {
                return;
            }

            if (__instance.Owner?.RunState is RunState runState)
            {
                SetModifiers(runState, __state);
            }
        }
    }
}
