using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class CursedOneRingPhone_Koishi : CustomCardModel, IUseAncientCardFace
    {
        private const string IntangibleAttackThresholdKey = "IntangibleAttackThreshold";

        public CursedOneRingPhone_Koishi()
            : base(3, CardType.Skill, CardRarity.Rare, TargetType.AllAllies, true)
        {
        }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new DynamicVar("DamageBonus", 4m),
            new DynamicVar(IntangibleAttackThresholdKey, 2m)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
        {
            HoverTipFactory.FromPower<IntangiblePower>()
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (base.CombatState == null)
            {
                return;
            }

            await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

            var allies = base.CombatState.GetTeammatesOf(base.Owner.Creature)
                .Where(c => c != null && c.IsAlive && c.IsPlayer && c != base.Owner.Creature)
                .ToList();

            decimal totalAttackThreshold = UpdateIntangibleAttackThreshold(allies.Count);
            await PowerCmd.Apply<MissMarysPhoneThisSidePower>(
                choiceContext,
                base.Owner.Creature,
                totalAttackThreshold,
                base.Owner.Creature,
                this,
                false);

            foreach (Creature ally in allies)
            {
                await PowerCmd.Apply<KuugaPower>(
                    choiceContext,
                    ally,
                    base.DynamicVars["DamageBonus"].BaseValue,
                    base.Owner.Creature,
                    this,
                    false);

                await PowerCmd.Apply<MissMarysPhoneThatSidePower>(
                    choiceContext,
                    ally,
                    1m,
                    base.Owner.Creature,
                    this,
                    false);
            }

            PlayerCmd.EndTurn(base.Owner, false, null);
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars["DamageBonus"].UpgradeValueBy(2m);
            UpdateIntangibleAttackThreshold();
        }

        protected override bool IsPlayable
        {
            get
            {
                UpdateIntangibleAttackThreshold();
                return base.IsPlayable;
            }
        }

        private decimal UpdateIntangibleAttackThreshold()
        {
            int allyCount = base.CombatState?
                .GetTeammatesOf(base.Owner.Creature)
                .Count(c => c != null && c.IsAlive && c.IsPlayer && c != base.Owner.Creature) ?? 1;
            return UpdateIntangibleAttackThreshold(allyCount);
        }

        private decimal UpdateIntangibleAttackThreshold(int allyCount)
        {
            decimal threshold = 2m * (allyCount + (base.IsUpgraded ? 0 : 1));
            base.DynamicVars[IntangibleAttackThresholdKey].BaseValue = threshold;
            return threshold;
        }
    }
}
