using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs;
using KomeijiKoishi.Multiplayer;

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
        private static readonly AccessTools.FieldRef<RunState, IReadOnlyList<ModifierModel>> ModifiersRef =
            AccessTools.FieldRefAccess<RunState, IReadOnlyList<ModifierModel>>("<Modifiers>k__BackingField");

        public static void Prefix(RunState state)
        {
            KoishiModConfig.BeginRunWithHostConfig(state);
            if (KoishiModConfig.HasSyncedRunConfigForCurrentRun())
            {
                ModifiersRef(state) = KoishiModConfig.WithActiveRunAncientConfig(state.Modifiers);
            }
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

    [HarmonyPatch(typeof(StartRunLobby), MethodType.Constructor, typeof(GameMode), typeof(MegaCrit.Sts2.Core.Multiplayer.Game.INetGameService), typeof(IStartRunLobbyListener), typeof(int))]
    public static class StartRunLobbyKoishiConfigSyncPatch
    {
        public static void Postfix(StartRunLobby __instance)
        {
            KoishiRunConfigSynchronizer.Register(__instance.NetService);
        }
    }

    [HarmonyPatch(typeof(StartRunLobby), "BeginRunForAllPlayers")]
    public static class BeginRunForAllPlayersKoishiConfigSyncPatch
    {
        public static void Prefix(StartRunLobby __instance, ref List<ModifierModel> __1)
        {
            __1 = KoishiModConfig.WithHostAncientConfig(__1).ToList();
            KoishiModConfig.BeginRunWithHostConfig(__1);
            KoishiRunConfigSynchronizer.BroadcastHostConfig(__instance.NetService);
        }
    }

    [HarmonyPatch(typeof(StartRunLobby), "HandleLobbyBeginRunMessage")]
    public static class ReceiveLobbyBeginRunKoishiConfigSyncPatch
    {
        public static void Prefix(LobbyBeginRunMessage message)
        {
            KoishiModConfig.BeginRunWithSyncedHostModifiers(message.modifiers.Select(ModifierModel.FromSerializable).ToList());
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
