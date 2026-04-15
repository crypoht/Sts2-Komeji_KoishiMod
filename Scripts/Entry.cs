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

        DanmakuPool.Register<ArrowDanmaku_Koishi>();
        DanmakuPool.Register<HeartDanmaku_Koishi>();
        DanmakuPool.Register<LargeOrbDanmaku_Koishi>();
        DanmakuPool.Register<RiceDanmaku_Koishi>();
        DanmakuPool.Register<SmallOrbDanmaku_Koishi>();
        DanmakuPool.Register<SquareDanmaku_Koishi>(); 
        DanmakuPool.Register<StarDanmaku_Koishi>();
        DanmakuPool.Register<YinYangOrbDanmaku_Koishi>();

        Log.Debug("Koishi Mod: Initialized successfully!");
    }
}