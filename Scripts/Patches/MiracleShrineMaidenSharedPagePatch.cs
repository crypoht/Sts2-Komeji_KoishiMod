using HarmonyLib;
using KomeijiKoishi.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;

namespace KomeijiKoishi.Patches
{
    [HarmonyPatch(typeof(EventSynchronizer), nameof(EventSynchronizer.IsShared), MethodType.Getter)]
    public static class MiracleShrineMaidenSharedPagePatch
    {
        private static readonly AccessTools.FieldRef<EventSynchronizer, EventModel?> CanonicalEvent =
            AccessTools.FieldRefAccess<EventSynchronizer, EventModel?>("_canonicalEvent");

        private static readonly AccessTools.FieldRef<EventSynchronizer, uint> PageIndex =
            AccessTools.FieldRefAccess<EventSynchronizer, uint>("_pageIndex");

        public static void Postfix(EventSynchronizer __instance, ref bool __result)
        {
            EventModel? canonicalEvent = CanonicalEvent(__instance);
            if (canonicalEvent is MiracleShrineMaiden_Koishi && PageIndex(__instance) > 0U)
            {
                __result = false;
            }
        }
    }
}
