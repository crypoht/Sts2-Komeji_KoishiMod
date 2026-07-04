using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers; 
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using KomeijiKoishi.Enums;
using System.Linq;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
        public sealed class HuaKai_Koishi : CustomCardModel
    {
        public HuaKai_Koishi() 
            : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true) { }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { KoishiTags.Stance };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[]
        {
            HoverTipFactory.FromKeyword(KoishiKeywords.Stance),
            HoverTipFactory.FromPower<BloomStancePower>(),
            HoverTipFactory.FromPower<ClosedStancePower>()
        };

        public override CardMultiplayerConstraint MultiplayerConstraint
        {
            get { return CardMultiplayerConstraint.MultiplayerOnly; }
        }
  

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as Player;
            
            if (player == null || base.CombatState == null) return;

            await CreatureCmd.TriggerAnim(player.Creature, "Cast", player.Character.CastAnimDelay);

            var teammates = base.CombatState.Players
                .Where(p => p != null && p != player && p.Creature != null && !p.Creature.IsDead && p.Creature.Side == player.Creature.Side)
                .ToList();

            foreach (var teammate in teammates)
            {
                await ApplyBloomHarmony(choiceContext, player, teammate.Creature);
            }
        }

        private async Task ApplyBloomHarmony(PlayerChoiceContext choiceContext, Player player, Creature target)
        {
            await ClearStancesLocally(target);
            if (target.Player != null)
            {
                int bonusEnergy = 0;
                var superego = target.Powers.FirstOrDefault(p => p is SuperegoPower);
                if (superego != null)
                {
                    bonusEnergy = (int)superego.Amount;
                }

                int totalEnergyGain = BloomStancePower.BloomEnergyGainAmount + bonusEnergy;
                if (totalEnergyGain > 0)
                {
                    await PlayerCmd.GainEnergy(totalEnergyGain, target.Player);
                }
            }

            await PowerCmd.Apply<BloomStancePower>(choiceContext, target, 1m, player.Creature, this, false);
            await PowerCmd.Apply<HuaKaiBloomHarmonyPower>(choiceContext, target, 1m, player.Creature, this, false);

            var harmonyPower = target.GetPower<HuaKaiBloomHarmonyPower>();
            harmonyPower?.BindSourceCard(this);
        }

        private async Task ClearStancesLocally(Creature targetCreature)
        {
            var powersToRemove = targetCreature.Powers
                .Where(p => p is BloomStancePower || p is ClosedStancePower)
                .ToList();

            foreach (var power in powersToRemove)
            {
                await PowerCmd.Remove(power); 
            }
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(-1); 
        }
    }
}
