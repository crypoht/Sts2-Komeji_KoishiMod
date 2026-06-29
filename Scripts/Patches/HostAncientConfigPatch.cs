using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs;

namespace KomeijiKoishi.Patches
{
    [HarmonyPatch(typeof(NGame), nameof(NGame.StartNewSingleplayerRun))]
    public static class SingleplayerAncientConfigPatch
    {
        public static void Prefix(ref IReadOnlyList<ModifierModel> modifiers)
        {
            modifiers = KoishiModConfig.WithHostAncientConfig(modifiers);
            KoishiModConfig.BeginRunWithHostConfig(modifiers);
        }
    }

    [HarmonyPatch(typeof(NGame), nameof(NGame.StartNewMultiplayerRun))]
    public static class HostAncientConfigPatch
    {
        public static void Prefix(StartRunLobby lobby, ref IReadOnlyList<ModifierModel> modifiers)
        {
            modifiers = KoishiModConfig.WithHostAncientConfig(modifiers);
            KoishiModConfig.BeginRunWithHostConfig(modifiers);
        }
    }

    [HarmonyPatch(typeof(RunManager), nameof(RunManager.SetUpNewSingleplayer))]
    public static class NewSingleplayerRunAncientConfigPatch
    {
        public static void Prefix(RunState state)
        {
            KoishiModConfig.BeginRunWithHostConfig(state);
        }
    }

    [HarmonyPatch(typeof(RunManager), nameof(RunManager.SetUpNewMultiplayer))]
    public static class NewMultiplayerRunAncientConfigPatch
    {
        public static void Prefix(RunState state)
        {
            KoishiModConfig.BeginRunWithHostConfig(state);
        }
    }


    [HarmonyPatch(typeof(RunManager), "EnterAct")]
    public static class TrackEnteringActForAncientConfigPatch
    {
        public static void Prefix(int currentActIndex)
        {
            KoishiModConfig.SetActiveRunCurrentActIndex(currentActIndex);
        }
    }

    [HarmonyPatch(typeof(RunManager), "SetActInternal")]
    public static class TrackCurrentActForAncientConfigPatch
    {
        private static readonly AccessTools.FieldRef<RunManager, RunState> RunStateRef =
            AccessTools.FieldRefAccess<RunManager, RunState>("<State>k__BackingField");

        public static void Postfix(RunManager __instance)
        {
            RunState? state = RunStateRef(__instance);
            if (state != null)
            {
                KoishiModConfig.SetActiveRunCurrentActIndex(state.CurrentActIndex);
            }
        }
    }
}
