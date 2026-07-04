using Godot;
using HarmonyLib;
using KomeijiKoishi.Cards;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs;

namespace KomeijiKoishi.Patches
{
    [HarmonyPatch(typeof(NGame), nameof(NGame._Input))]
    public static class MoriyaDancePlaybackHotkeyPatch
    {
        public static void Postfix(InputEvent inputEvent)
        {
            if (inputEvent is not InputEventKey { Pressed: true, Echo: false } keyEvent)
            {
                return;
            }

            bool zAndLPressed = keyEvent.Keycode == Key.L && Input.IsKeyPressed(Key.Z)
                                || keyEvent.Keycode == Key.Z && Input.IsKeyPressed(Key.L);
            if (keyEvent.IsShiftPressed() && zAndLPressed)
            {
                MoriyaDance_Koishi.BlockPlaybackForCurrentRun();
                MegaCrit.Sts2.Core.Logging.Log.Info("[Koishi] Moriya Dance playback disabled locally for this run.");
            }
        }
    }

    [HarmonyPatch(typeof(RunManager), nameof(RunManager.SetUpNewSingleplayer))]
    public static class MoriyaDanceSingleplayerPlaybackResetPatch
    {
        public static void Prefix()
        {
            MoriyaDance_Koishi.ResetLocalPlaybackBlockerForNewRun();
        }
    }

    [HarmonyPatch(typeof(RunManager), nameof(RunManager.SetUpNewMultiplayer))]
    public static class MoriyaDanceMultiplayerPlaybackResetPatch
    {
        public static void Prefix()
        {
            MoriyaDance_Koishi.ResetLocalPlaybackBlockerForNewRun();
        }
    }
}
