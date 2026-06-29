using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Events;

namespace KomeijiKoishi.Patches
{
    [HarmonyPatch(typeof(ColorfulPhilosophers))]
    public static class ColorfulPhilosophersPatch
    {
        [HarmonyPatch("CardPoolColorOrder", MethodType.Getter)]
        [HarmonyPostfix]
        public static void Postfix(ref IEnumerable<CardPoolModel> __result)
        {
            __result = __result.Append(ModelDb.CardPool<KoishiCardPool>());
        }
    }
}
