using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Enums;
using KomeijiKoishi.Utils_Koishi;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class BigHug_Koishi : CustomCardModel
    {
        private const int MaxEnergyGain = 4;

        public BigHug_Koishi()
            : base(3, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
        {
        }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new List<IHoverTip>
        {
            HoverTipFactory.FromKeyword(KoishiKeywords.Unconscious)
        };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new DamageVar(31m, ValueProp.Move)
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
                var player = base.Owner as Player;
                if (player == null) return;
                
                await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                    .FromCard(this)
                    .Targeting(cardPlay.Target)
                    .WithHitFx("vfx/vfx_attack_blunt") 
                    .Execute(choiceContext);

                int energyGain = Math.Min(MaxEnergyGain, CountUnconsciousCardsInDiscard(player));
                if (energyGain > 0)
                {
                    await PlayerCmd.GainEnergy(energyGain, player);
                }
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[BigHug_Koishi] OnPlay Error: {e.Message}");
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Damage.UpgradeValueBy(10m);
        }

        protected override bool ShouldGlowGoldInternal
        {
            get
            {
                return base.Owner is Player player && CountUnconsciousCardsInDiscard(player) > 0;
            }
        }

        private static int CountUnconsciousCardsInDiscard(Player player)
        {
            var discardPile = PileType.Discard.GetPile(player);
            return discardPile?.Cards.Count(c => KoishiExtensions.IsTrulyUnconscious(c)) ?? 0;
        }
    }
}
