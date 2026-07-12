using Godot;

namespace KomeijiKoishi.Cards;

public static class KoishiImagePaths
{
    private const string CardImageDirectory = "res://mods/Komeiji_Koishi/images/cards/";
    private const string DanmakuProjectileImageDirectory = "res://mods/Komeiji_Koishi/images/danmaku_projectiles/";

    public static string CardNormalPortrait(Type cardType)
    {
        return CardImageDirectory + cardType.Name + ".png";
    }

    public static string CardFumoPortrait(Type cardType)
    {
        return CardImageDirectory + cardType.Name + "_fumo.png";
    }

    public static string CardAncientPortrait(Type cardType)
    {
        return CardImageDirectory + cardType.Name + "_ancient.png";
    }

    public static string CardPortrait(Type cardType)
    {
        string normalPath = CardNormalPortrait(cardType);
        if (!KoishiModConfig.UseFumoCardArt)
        {
            return normalPath;
        }

        string fumoPath = CardFumoPortrait(cardType);
        return ResourceLoader.Exists(fumoPath) ? fumoPath : normalPath;
    }

    public static string DanmakuProjectile(string imageName)
    {
        return DanmakuProjectileImageDirectory + imageName + ".png";
    }
}
