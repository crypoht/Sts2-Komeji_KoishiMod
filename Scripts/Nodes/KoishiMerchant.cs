using Godot;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace KomeijiKoishi.Scripts.Nodes
{
	public partial class KoishiMerchant : NMerchantCharacter
	{
		public override void _Ready()
		{
            // The legacy merchant uses a static Sprite2D, not a SpineSprite.
		}

		public new void PlayAnimation(string anim, bool loop = false)
		{
		   
		}


	}
}
