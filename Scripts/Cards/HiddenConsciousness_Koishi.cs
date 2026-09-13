using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Combat;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Enums;
using KomeijiKoishi.Utils_Koishi;
using KomeijiKoishi.Cards.Danmaku;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class HiddenConsciousness_Koishi : CustomCardModel
    {
        public HiddenConsciousness_Koishi() 
            : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true) { }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new BlockVar(KomeijiKoishi.Config.KoishiBalanceManager.Value(15m, 11m), ValueProp.Move),
            new DynamicVar("Magic", 3m)
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try
            {
                var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
                var combatState = base.CombatState as CombatState;
                if (player == null || combatState == null) return;

                await CreatureCmd.GainBlock(player.Creature, base.DynamicVars.Block.BaseValue, ValueProp.Move, cardPlay, false);

                int generateCount = base.DynamicVars["Magic"].IntValue;
                for (int i = 0; i < generateCount; i++)
                {
                    var heartDanmaku = combatState.CreateCard<HeartDanmaku_Koishi>(player);
                    DanmakuPool.InheritEnchantment(this, heartDanmaku);

                    await CardPileCmd.AddGeneratedCardToCombat(
                        heartDanmaku,
                        PileType.Exhaust,
                        player,
                        CardPilePosition.Bottom);

                    var aliveEnemies = combatState.HittableEnemies
                        .Where(enemy => enemy != null && !enemy.IsDead)
                        .ToList();
                    if (aliveEnemies.Count == 0)
                    {
                        break;
                    }

                    var target = player.RunState.Rng.Shuffle.NextItem(aliveEnemies);
                    await KoishiExtensions.SafeAutoPlayCard(
                        choiceContext,
                        player,
                        heartDanmaku,
                        target,
                        AutoPlayType.Default,
                        true,
                        false);
                }
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[HiddenConsciousness_Koishi] Error: {e.Message}");
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Block.UpgradeValueBy(4m);
            base.DynamicVars["Magic"].UpgradeValueBy(1m);
        }
    }
}
