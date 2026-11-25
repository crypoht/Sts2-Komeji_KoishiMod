using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace KomeijiKoishi.Relics;

public abstract class KoishiCollaborationRelic : CustomRelicModel
{
    private const string IconDirectory = "res://mods/Komeiji_Koishi/images/relics/";
    private const string Placeholder = IconDirectory + "temp.png";

    public sealed override RelicRarity Rarity => RelicRarity.Ancient;

    public override string PackedIconPath
    {
        get
        {
            string path = IconDirectory + GetType().Name + ".png";
            return ResourceLoader.Exists(path, "Texture2D") ? path : Placeholder;
        }
    }

    protected override string PackedIconOutlinePath => PackedIconPath;
    protected override string BigIconPath => PackedIconPath;
}
