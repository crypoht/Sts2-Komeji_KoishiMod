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
using System.Linq;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
        public sealed class HuaKai_Koishi : CustomCardModel
    {
        public HuaKai_Koishi() 
            : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true) { }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

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
                await ApplyRandomKoishiPower(choiceContext, player, teammate.Creature);
                await AutoPlayRandomTeammateCard(choiceContext, teammate);
            }
        }

        private async Task ApplyRandomKoishiPower(PlayerChoiceContext choiceContext, Player player, Creature target)
        {
            List<int> powerPool = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };
            int randomPowerIndex = player.RunState.Rng.Shuffle.NextItem(powerPool);

            switch (randomPowerIndex)
            {
                case 0: 
                    await ClearStancesLocally(target);
                    await PowerCmd.Apply<BloomStancePower>(choiceContext,target, 1m, player.Creature, this, false); 
                    break;
                case 1: await PowerCmd.Apply<BramblyRoseGardenPower>(choiceContext,target, 1m, player.Creature, this, false); break;
                case 2: 
                    await ClearStancesLocally(target);
                    await PowerCmd.Apply<ClosedStancePower>(choiceContext,target, 1m, player.Creature, this, false); 
                    break;
                case 3: await PowerCmd.Apply<ConsciousnessSpiralPower>(choiceContext,target, 1m, player.Creature, this, false); break;
                case 4: await PowerCmd.Apply<DanmakuArtPower>(choiceContext,target, 2m, player.Creature, this, false); break;
                case 5: await PowerCmd.Apply<FetusDreamPower>(choiceContext,target, 1m, player.Creature, this, false); break;
                case 6: await PowerCmd.Apply<FlawOfDNAPower>(choiceContext,target, 1m, player.Creature, this, false); break;
                case 7: await PowerCmd.Apply<InstinctiveFormPower>(choiceContext,target, 1m, player.Creature, this, false); break;
                case 8: await PowerCmd.Apply<JoyMaskPower>(choiceContext,target, 1m, player.Creature, this, false); break;
                case 9: await PowerCmd.Apply<KoishiKokoroPower>(choiceContext,target, 1m, player.Creature, this, false); break;
                case 10: await PowerCmd.Apply<KoishiKokoroPower>(choiceContext,target, 1m, player.Creature, this, false); break;
                case 11: await PowerCmd.Apply<KuugaPower>(choiceContext,target, 1m, player.Creature, this, false); break;
                case 12: await PowerCmd.Apply<MentalStellarSuccessionPower>(choiceContext,target, 100m, player.Creature, this, false); break;
                case 13: await PowerCmd.Apply<MiraclePower>(choiceContext,target, 1m, player.Creature, this, false); break;
                case 14: await PowerCmd.Apply<PerfectMindControlPower>(choiceContext,target, 1m, player.Creature, this, false); break;
                case 15: await PowerCmd.Apply<PhilosophyOfTheHatedPower>(choiceContext,target, 2m, player.Creature, this, false); break;
                case 16: await PowerCmd.Apply<RecognizedGeniusPower>(choiceContext,target, 1m, player.Creature, this, false); break;
                case 17: await PowerCmd.Apply<ReimuHelpPower>(choiceContext,target, 1m, player.Creature, this, false); break;
                case 18: await PowerCmd.Apply<RoseProtectionPower>(choiceContext,target, 40m, player.Creature, this, false); break;
                case 19: await PowerCmd.Apply<SecondNeedPower>(choiceContext,target, 9m, player.Creature, this, false); break;
                case 20: await PowerCmd.Apply<SelflessLovePower>(choiceContext,target, 1m, player.Creature, this, false); break;
                case 21: await PowerCmd.Apply<SuperegoPower>(choiceContext,target, 1m, player.Creature, this, false); break;
                case 22: await PowerCmd.Apply<TracingPower>(choiceContext,target, 514m, player.Creature, this, false); break;
                case 23: await PowerCmd.Apply<UltimateUnconsciousFormPower>(choiceContext,target, 1m, player.Creature, this, false); break;
                case 24: await PowerCmd.Apply<UndergroundRosePower>(choiceContext,target, 1m, player.Creature, this, false); break;
            }
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

        private async Task AutoPlayRandomTeammateCard(PlayerChoiceContext choiceContext, Player teammate)
        {
            var handPile = PileType.Hand.GetPile(teammate);
            if (handPile == null || handPile.Cards.Count == 0) return;

            var playableCards = handPile.Cards
                .Where(card => card.Keywords == null || !card.Keywords.Contains(CardKeyword.Unplayable))
                .ToList();

            if (playableCards.Count == 0) return;

            CardModel? targetCard = teammate.RunState.Rng.CombatCardSelection.NextItem(playableCards);
            if (targetCard == null) return;

            await KomeijiKoishi.Utils_Koishi.KoishiExtensions.SafeAutoPlayCard(choiceContext, teammate, targetCard);
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(-1); 
        }
    }
}
