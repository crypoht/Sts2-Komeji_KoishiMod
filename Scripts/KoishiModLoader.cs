using Godot;
using HarmonyLib;

namespace KomeijiKoishi
{
    public partial class KoishiModLoader : Node
    {
        public override void _Ready()
        {
            var harmony = new Harmony("com.komeijikoishi.mod");
            harmony.PatchAll();
            GD.Print("古明地恋 Mod 加载成功！");
        }
    }
}