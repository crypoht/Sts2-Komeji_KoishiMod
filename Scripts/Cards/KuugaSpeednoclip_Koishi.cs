using System.Collections.Generic;
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
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class KuugaSpeednoclip_Koishi : CustomCardModel
    {
        public KuugaSpeednoclip_Koishi()
            : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
        {
        }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new CalculationBaseVar(0m),
            new ExtraDamageVar(1m),
            new CalculatedDamageVar(ValueProp.Move).WithMultiplier(GetPositiveKuugaStacks),
            new RepeatVar(3)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[]
        {
            HoverTipFactory.FromPower<KuugaPower>()
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target == null)
            {
                return;
            }

            await DamageCmd.Attack(base.DynamicVars.CalculatedDamage)
                .WithHitCount(base.DynamicVars.Repeat.IntValue)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }

        private static decimal GetPositiveKuugaStacks(MegaCrit.Sts2.Core.Models.CardModel card, Creature? _)
        {
            decimal kuugaAmount = card.Owner?.Creature.GetPower<KuugaPower>()?.Amount ?? 0m;
            return decimal.Max(0m, kuugaAmount);
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.ExtraDamage.UpgradeValueBy(1m);
        }
    }
}
