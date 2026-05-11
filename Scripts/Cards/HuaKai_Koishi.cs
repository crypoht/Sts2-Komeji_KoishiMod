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
            : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        public override CardMultiplayerConstraint MultiplayerConstraint
        {
            get { return CardMultiplayerConstraint.MultiplayerOnly; }
        }
  

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as Player;
            var target = cardPlay.Target; 
            
            if (player == null || target == null) return;

            await CreatureCmd.TriggerAnim(player.Creature, "Cast", player.Character.CastAnimDelay);

            List<int> powerPool = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };
            int randomPowerIndex = player.RunState.Rng.Shuffle.NextItem(powerPool);

            switch (randomPowerIndex)
            {
                case 0: 
                    await ClearStancesLocally(target);
                    await PowerCmd.Apply<BloomStancePower>(target, 1m, player.Creature, this, false); 
                    break;
                case 1: await PowerCmd.Apply<BramblyRoseGardenPower>(target, 100m, player.Creature, this, false); break;
                case 2: 
                    await ClearStancesLocally(target);
                    await PowerCmd.Apply<ClosedStancePower>(target, 1m, player.Creature, this, false); 
                    break;
                case 3: await PowerCmd.Apply<ConsciousnessSpiralPower>(target, 1m, player.Creature, this, false); break;
                case 4: await PowerCmd.Apply<DanmakuArtPower>(target, 2m, player.Creature, this, false); break;
                case 5: await PowerCmd.Apply<FetusDreamPower>(target, 1m, player.Creature, this, false); break;
                case 6: await PowerCmd.Apply<FlawOfDNAPower>(target, 1m, player.Creature, this, false); break;
                case 7: await PowerCmd.Apply<InstinctiveFormPower>(target, 1m, player.Creature, this, false); break;
                case 8: await PowerCmd.Apply<JoyMaskPower>(target, 1m, player.Creature, this, false); break;
                case 9: await PowerCmd.Apply<KoishiKokoroPower>(target, 1m, player.Creature, this, false); break;
                case 10: await PowerCmd.Apply<KoishiKokoroPower>(target, 1m, player.Creature, this, false); break;
                case 11: await PowerCmd.Apply<KuugaPower>(target, 1m, player.Creature, this, false); break;
                case 12: await PowerCmd.Apply<MentalStellarSuccessionPower>(target, 100m, player.Creature, this, false); break;
                case 13: await PowerCmd.Apply<MiraclePower>(target, 1m, player.Creature, this, false); break;
                case 14: await PowerCmd.Apply<PerfectMindControlPower>(target, 1m, player.Creature, this, false); break;
                case 15: await PowerCmd.Apply<PhilosophyOfTheHatedPower>(target, 2m, player.Creature, this, false); break;
                case 16: await PowerCmd.Apply<RecognizedGeniusPower>(target, 1m, player.Creature, this, false); break;
                case 17: await PowerCmd.Apply<ReimuHelpPower>(target, 1m, player.Creature, this, false); break;
                case 18: await PowerCmd.Apply<RoseProtectionPower>(target, 40m, player.Creature, this, false); break;
                case 19: await PowerCmd.Apply<SecondNeedPower>(target, 9m, player.Creature, this, false); break;
                case 20: await PowerCmd.Apply<SelflessLovePower>(target, 1m, player.Creature, this, false); break;
                case 21: await PowerCmd.Apply<SuperegoPower>(target, 1m, player.Creature, this, false); break;
                case 22: await PowerCmd.Apply<TracingPower>(target, 514m, player.Creature, this, false); break;
                case 23: await PowerCmd.Apply<UltimateUnconsciousFormPower>(target, 1m, player.Creature, this, false); break;
                case 24: await PowerCmd.Apply<UndergroundRosePower>(target, 1m, player.Creature, this, false); break;
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
        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(-1); 
        }
    }
}