using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using KomeijiKoishi.Cards.Danmaku;
namespace KomeijiKoishi.Scripts;

[ModInitializer("Init")]
public class Entry
{
    public static void Init()
    {
        var harmony = new Harmony("kingof.koishimod");
        harmony.PatchAll();

        ScriptManagerBridge.LookupScriptsInAssembly(typeof(Entry).Assembly);

        Log.Debug("Koishi Mod: Initialized successfully!");
    }
}