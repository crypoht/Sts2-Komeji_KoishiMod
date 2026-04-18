using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils; 
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers; 
using MegaCrit.Sts2.Core.GameActions.Multiplayer; 
using KomeijiKoishi.Pools; 
using KomeijiKoishi.Enums;
using KomeijiKoishi.Powers; 
namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class UnconsciousPassage_Koishi : CustomCardModel
    {
        public UnconsciousPassage_Koishi() 
            : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";
        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { KoishiTags.Subconscious };
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { KoishiKeywords.Closed };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DynamicVar("Blur", 2m) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try
            {
                var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
                if (player == null) return;

                if (player.Character != null)
                {
                    await CreatureCmd.TriggerAnim(player.Creature, "Buff", player.Character.CastAnimDelay);
                }

                await PowerCmd.Apply<BlurPower>(
                    player.Creature, 
                    base.DynamicVars["Blur"].BaseValue, 
                    player.Creature, 
                    this, 
                    false
                );

                await Cmd.Wait(0.2f, false);

                await ClosedStancePower.EnterThisStance(choiceContext, player, this);
            }
            catch (Exception ex)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[UnconsciousPassage] Error: {ex.Message}");
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars["Blur"].UpgradeValueBy(1m);
        }
    }
}