using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Enums;
using KomeijiKoishi.Vfx;
using MegaCrit.Sts2.Core.Helpers;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(TokenCardPool))]
    public sealed class YinYangGhostGodOrb_Koishi : CustomCardModel
    {
        private const string ExtraEnemyHitsKey = "ExtraEnemyHits";
        private const string LoneEnemyMultiplierKey = "LoneEnemyMultiplier";

        public YinYangGhostGodOrb_Koishi()
            : base(3, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy, true)
        {
        }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag>
        {
            KoishiTags.Danmaku
        };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new DamageVar(23m, ValueProp.Move),
            new DynamicVar(ExtraEnemyHitsKey, 1m),
            new DynamicVar(LoneEnemyMultiplierKey, 3m)
        };

        public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
        {
            if (card != this || this.CountLivingEnemies() != 1)
            {
                modifiedCost = originalCost;
                return false;
            }

            modifiedCost = 0m;
            return true;
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target);

            int enemyCount = this.CountLivingEnemies();
            decimal damage = base.DynamicVars.Damage.BaseValue;
            int hitCount = Math.Max(1, enemyCount + base.DynamicVars[ExtraEnemyHitsKey].IntValue);

            if (enemyCount == 1)
            {
                damage *= base.DynamicVars[LoneEnemyMultiplierKey].BaseValue;
                hitCount = 1;
            }

            NYinYangGhostGodOrbAttackVfx? vfx = NYinYangGhostGodOrbAttackVfx.Create(base.Owner.Creature, cardPlay.Target);
            if (vfx != null)
            {
                NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx);
                float totalVfxDuration =
                    NYinYangGhostGodOrbAttackVfx.FanSpreadDuration +
                    NYinYangGhostGodOrbAttackVfx.FanHoldDuration +
                    NYinYangGhostGodOrbAttackVfx.FanReturnDurationPerOrb * NYinYangGhostGodOrbAttackVfx.FanOrbCount +
                    NYinYangGhostGodOrbAttackVfx.FinalProjectileDuration;
                await Cmd.Wait(totalVfxDuration, false);
            }

            for (int i = 0; i < hitCount; i++)
            {
                await DamageCmd.Attack(damage)
                    .FromCard(this)
                    .Targeting(cardPlay.Target)
                    .WithHitFx("vfx/vfx_attack_blunt")
                    .Execute(choiceContext);
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Damage.UpgradeValueBy(4m);
        }

        private int CountLivingEnemies()
        {
            return base.CombatState?.HittableEnemies.Count(enemy => !enemy.IsDead) ?? 0;
        }
    }
}
