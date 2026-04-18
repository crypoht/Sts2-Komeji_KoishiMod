using MegaCrit.Sts2.addons.mega_text;

namespace KomeijiKoishi.Scripts.Nodes;

public partial class KoishiMegaLabel : MegaLabel
{
		
		public override void _Ready()
		{
		  
			base._Ready();

			AutoSizeEnabled = false;

			AddThemeFontSizeOverride("font_size", 37);
		}
	
	
	
}
