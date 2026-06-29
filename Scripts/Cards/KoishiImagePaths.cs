using Godot;

namespace KomeijiKoishi.Cards;

public static class KoishiImagePaths
{
    private const string CardImageDirectory = "res://mods/Komeiji_Koishi/images/cards/";
    private const string DanmakuProjectileImageDirectory = "res://mods/Komeiji_Koishi/images/danmaku_projectiles/";

    public static string CardPortrait(Type cardType)
    {
        string normalPath = CardImageDirectory + cardType.Name + ".png";
        if (!KoishiModConfig.UseFumoCardArt)
        {
            return normalPath;
        }

        string fumoPath = CardImageDirectory + cardType.Name + "_fumo.png";
        return ResourceLoader.Exists(fumoPath) ? fumoPath : normalPath;
    }

    public static string DanmakuProjectile(string imageName)
    {
        return DanmakuProjectileImageDirectory + imageName + ".png";
    }
}
