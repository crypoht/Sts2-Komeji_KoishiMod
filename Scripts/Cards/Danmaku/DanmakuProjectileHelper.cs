using KomeijiKoishi.Vfx;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace KomeijiKoishi.Cards.Danmaku;

public static class DanmakuProjectileHelper
{
    public static NDanmakuProjectileVfx? Create(Creature source, Creature target, string imageName)
    {
        return NDanmakuProjectileVfx.Create(
            source,
            target,
            KoishiImagePaths.DanmakuProjectile(imageName),
            NDanmakuProjectileVfx.DefaultDuration,
            GetVisualWidth(imageName));
    }

    public static void AddToCombat(Creature source, Creature target, string imageName)
    {
        NDanmakuProjectileVfx? vfx = Create(source, target, imageName);
        if (vfx != null)
        {
            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx);
            Log.Debug($"[KoishiDanmakuProjectile] Added projectile vfx: {imageName}");
        }
        else
        {
            Log.Warn($"[KoishiDanmakuProjectile] Failed to create projectile vfx: {imageName}");
        }
    }

    private static float GetVisualWidth(string imageName)
    {
        return imageName switch
        {
            "yinyangorb" => 240f,
            "largeorb" => 120f,
            _ => 80f
        };
    }
}
