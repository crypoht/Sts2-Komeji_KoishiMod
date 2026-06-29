using HarmonyLib;
using KomeijiKoishi.Modifiers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;

namespace KomeijiKoishi.Patches
{
    [HarmonyPatch(typeof(NTopBar), nameof(NTopBar.Initialize))]
    public static class TopBarHiddenModifierPatch
    {
        private static readonly Action<RunState, IReadOnlyList<ModifierModel>> SetModifiers =
            (Action<RunState, IReadOnlyList<ModifierModel>>)Delegate.CreateDelegate(
                typeof(Action<RunState, IReadOnlyList<ModifierModel>>),
                AccessTools.PropertySetter(typeof(RunState), nameof(RunState.Modifiers)));

        public static void Prefix(IRunState runState, ref IReadOnlyList<ModifierModel>? __state)
        {
            if (runState is not RunState mutableRunState)
            {
                return;
            }

            IReadOnlyList<ModifierModel> modifiers = mutableRunState.Modifiers;
            List<ModifierModel> visibleModifiers = modifiers
                .Where(m => m is not DisableKoishiAncientWeightsModifier and not DisableKoishiAncientsModifier)
                .ToList();

            if (visibleModifiers.Count == modifiers.Count)
            {
                return;
            }

            __state = modifiers;
            SetModifiers(mutableRunState, visibleModifiers);
        }

        public static void Postfix(IRunState runState, IReadOnlyList<ModifierModel>? __state)
        {
            if (__state != null && runState is RunState mutableRunState)
            {
                SetModifiers(mutableRunState, __state);
            }
        }
    }
}
