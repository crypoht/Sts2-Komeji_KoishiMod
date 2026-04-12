using System.Collections.Generic;
using Godot; 
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.CardPools;
using KomeijiKoishi.Cards;
using BaseLib.Abstracts; 
using MegaCrit.Sts2.Core.Entities.Characters; 
using KomeijiKoishi.Pools;
using KomeijiKoishi.Relics;
using MegaCrit.Sts2.Core.Nodes.RestSite;

namespace KomeijiKoishi.Characters
{
	public class KoishiCharacter : PlaceholderCharacterModel
	{
		public override int StartingHp => 70; 
		
		public override Color NameColor => new(0.3f, 0.7f, 0.3f); 
		public override Color EnergyLabelOutlineColor => new(0.1f, 0.4f, 0.1f); 
		
		public override CharacterGender Gender => CharacterGender.Feminine;

		public override string CustomCharacterSelectIconPath => "res://mods/Komeiji_Koishi/images/char_select_koishi.png";
		
		public override string CustomCharacterSelectLockedIconPath => "res://mods/Komeiji_Koishi/images/char_select_koishi.png";
		
		public override string CustomCharacterSelectBg => "res://mods/Komeiji_Koishi/scenes/char_select_bg.tscn";
		
		public override string CustomIconTexturePath => "res://mods/Komeiji_Koishi/images/ui/koishi_icon.png";

		public override string CustomIconPath => "res://mods/Komeiji_Koishi/scenes/koishi_icon.tscn";

		public override string CustomMapMarkerPath => "res://mods/Komeiji_Koishi/images/ui/koishi_icon.png";

		public override string CustomVisualPath => "res://mods/Komeiji_Koishi/scenes/koishi_character.tscn";
		public override string CustomMerchantAnimPath => "res://mods/Komeiji_Koishi/scenes/koishi_merchant.tscn";
		public override string CustomRestSiteAnimPath => "res://mods/Komeiji_Koishi/scenes/koishi_rest_site.tscn";

		public override string CustomEnergyCounterPath => "res://mods/Komeiji_Koishi/scenes/koishi_energy_counter.tscn";

		public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";

		public override CardPoolModel CardPool => ModelDb.CardPool<KoishiCardPool>();
		public override RelicPoolModel RelicPool => ModelDb.RelicPool<KoishiRelicPool>();
		public override PotionPoolModel PotionPool => ModelDb.PotionPool<KoishiPotionPool>();

		public override IEnumerable<CardModel> StartingDeck => new List<CardModel>
		{
			ModelDb.Card<Strike_Koishi>(),
			ModelDb.Card<Strike_Koishi>(),
			ModelDb.Card<Strike_Koishi>(),
			ModelDb.Card<Strike_Koishi>(),
			ModelDb.Card<Defend_Koishi>(),
   			ModelDb.Card<Defend_Koishi>(),
			ModelDb.Card<Defend_Koishi>(),
   			ModelDb.Card<Defend_Koishi>(),
			ModelDb.Card<InstinctiveStab_Koishi>(),
			ModelDb.Card<Roaming_Koishi>()
		};

		 public override IReadOnlyList<RelicModel> StartingRelics => new List<RelicModel>
		{
			ModelDb.Relic<KoishiStarterRelic>()
		};

		public override MegaCrit.Sts2.Core.Animation.CreatureAnimator GenerateAnimator(MegaCrit.Sts2.Core.Bindings.MegaSpine.MegaSprite controller)
		{
			return null!; 
		}

		public override List<string> GetArchitectAttackVfx() => new List<string>
		{
			"vfx/vfx_attack_blunt",
			"vfx/vfx_heavy_blunt",
            "vfx/vfx_attack_slash"
		};
	}
}
