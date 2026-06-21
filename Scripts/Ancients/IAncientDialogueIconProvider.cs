using Godot;
using MegaCrit.Sts2.Core.Entities.Ancients;

namespace KomeijiKoishi.Ancients
{
    public interface IAncientDialogueIconProvider
    {
        Texture2D? GetDialogueIcon(AncientDialogueLine line);

        Texture2D? GetDialogueIconOutline(AncientDialogueLine line);
    }
}
