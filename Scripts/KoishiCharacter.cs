using System.Collections.Generic;
using Godot; 
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.CardPools;
using KomeijiKoishi.Cards;
using BaseLib.Abstracts; 
using MegaCrit.Sts2.Core.Entities.Characters; 
using KomeijiKoishi.Pools;
using KomeijiKoishi.Relics;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Models.Relics;
using BaseLib.Patches.UI;


namespace KomeijiKoishi.Characters
{
	public class KoishiCharacter : PlaceholderCharacterModel
	{
		public static string GetHeavyAnimIfApplicable(CharacterModel character) =>
			character is KoishiCharacter ? "heavyAttack" : "Attack";

		public static float GetHeavyAttackDelayIfApplicable(CharacterModel character) =>
			character is KoishiCharacter ? 0.2f : character.AttackAnimDelay;

		#if STS2_BETA
		public override CreatureAnimator GenerateAnimator(MegaSprite controller, Creature creature)
		#else
		public override CreatureAnimator GenerateAnimator(MegaSprite controller)
		#endif
		{
			AnimState idle = new("idle_loop", true);
			AnimState cast = new("cast", false);
			AnimState attack = new("attack", false);
			AnimState heavyAttack = new("attack_heavy", false);
			AnimState hurt = new("hurt", false);
			AnimState die = new("die", false);
			AnimState relaxed = new("relaxed_loop", true);

			cast.NextState = idle;
			attack.NextState = idle;
			heavyAttack.NextState = idle;
			hurt.NextState = idle;
			relaxed.AddBranch("Idle", idle, null);

			CreatureAnimator animator = new(idle, controller);
			animator.AddAnyState("Idle", idle, null);
			animator.AddAnyState("Dead", die, null);
			animator.AddAnyState("Hit", hurt, null);
			animator.AddAnyState("Attack", attack, null);
			animator.AddAnyState("heavyAttack", heavyAttack, null);
			animator.AddAnyState("Cast", cast, null);
			animator.AddAnyState("PowerUp", cast, null);
			animator.AddAnyState("Relaxed", relaxed, null);
			return animator;
		}
		public override int StartingHp => 67; 
		
		public override Color NameColor => new(0.3f, 0.7f, 0.3f); 
		public override Color EnergyLabelOutlineColor => new(0.624f, 0.271f, 0.463f);

		public override Color MapDrawingColor => new(0.486f, 0.988f, 0f);
		
		public override CharacterGender Gender => CharacterGender.Feminine;

		public override string CustomCharacterSelectIconPath => "res://mods/Komeiji_Koishi/images/char_select_koishi.png";
		
		public override string CustomCharacterSelectLockedIconPath => "res://mods/Komeiji_Koishi/images/char_select_koishi.png";
		
		public override string CustomCharacterSelectBg => "res://mods/Komeiji_Koishi/scenes/char_select_bg.tscn";
		
		public override string CustomIconTexturePath => "res://mods/Komeiji_Koishi/images/ui/koishi_icon.png";

		public override string CustomIconPath => "res://mods/Komeiji_Koishi/scenes/koishi_icon.tscn";

		public override string CustomMapMarkerPath => "res://mods/Komeiji_Koishi/images/ui/koishi_icon.png";

		public override string CustomVisualPath => KoishiModConfig.EffectiveSpineMode == KoishiSpineMode.PremiumAnimation
            ? "res://mods/Komeiji_Koishi/scenes/koishi_character_2.tscn"
            : "res://mods/Komeiji_Koishi/scenes/koishi_character.tscn";
		public override string CustomMerchantAnimPath => KoishiModConfig.EffectiveSpineMode == KoishiSpineMode.PremiumAnimation
            ? "res://mods/Komeiji_Koishi/scenes/koishi_merchant_2.tscn"
            : "res://mods/Komeiji_Koishi/scenes/koishi_merchant.tscn";
		public override string CustomRestSiteAnimPath => KoishiModConfig.EffectiveSpineMode == KoishiSpineMode.PremiumAnimation
            ? "res://mods/Komeiji_Koishi/scenes/koishi_rest_site_2.tscn"
            : "res://mods/Komeiji_Koishi/scenes/koishi_rest_site.tscn";

		public override string CustomEnergyCounterPath => "res://mods/Komeiji_Koishi/scenes/koishi_energy_counter.tscn";

        protected override IEnumerable<string> ExtraAssetPaths => new[]
        {
            "res://mods/Komeiji_Koishi/scenes/koishi_character.tscn",
            "res://mods/Komeiji_Koishi/scenes/koishi_character_2.tscn",
            "res://mods/Komeiji_Koishi/scenes/koishi_merchant.tscn",
            "res://mods/Komeiji_Koishi/scenes/koishi_merchant_2.tscn",
            "res://mods/Komeiji_Koishi/scenes/koishi_rest_site.tscn",
            "res://mods/Komeiji_Koishi/scenes/koishi_rest_site_2.tscn"
        };

		public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";
		public override string CustomArmPointingTexturePath => "res://mods/Komeiji_Koishi/images/hands/up.png";
		public override string CustomArmRockTexturePath => "res://mods/Komeiji_Koishi/images/hands/koishiya.png";
		public override string CustomArmPaperTexturePath => "res://mods/Komeiji_Koishi/images/hands/bu.png";
		public override string CustomArmScissorsTexturePath => "res://mods/Komeiji_Koishi/images/hands/jiandao.png";
		public override string CharacterSelectSfx => "res://mods/Komeiji_Koishi/sound/wuyishi.mp3";
		

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

		public override BaseLib.Patches.UI.RelicIconData? CustomYummyCookie => new BaseLib.Patches.UI.RelicIconData(
			BigIconPath: "res://mods/Komeiji_Koishi/images/relics/cookie_koishi.png",
			PackedIconPath: "res://mods/Komeiji_Koishi/images/relics/cookie_koishi.png",
			PackedIconOutlinePath: "res://mods/Komeiji_Koishi/images/relics/cookie_koishi.png"
		);
		public override List<string> GetArchitectAttackVfx() => new List<string>
		{
			"vfx/vfx_attack_blunt",
			"vfx/vfx_heavy_blunt",
            "vfx/vfx_attack_slash"
		};
	}
}
