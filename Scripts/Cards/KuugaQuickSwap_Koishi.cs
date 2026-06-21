using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class KuugaQuickSwap_Koishi : CustomCardModel
    {
        private const string KuugaDamageBonusKey = "KuugaDamageBonus";

        public KuugaQuickSwap_Koishi()
            : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
        {
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new DamageVar(0m, ValueProp.Move),
            new RepeatVar(3),
            new DynamicVar(KuugaDamageBonusKey, 1m)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[]
        {
            HoverTipFactory.FromPower<KuugaPower>()
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target == null || base.Owner is not Player player)
            {
                return;
            }

            decimal kuugaAmount = player.Creature.GetPower<KuugaPower>()?.Amount ?? 0m;
            decimal kuugaDamageBonus = decimal.Max(0m, kuugaAmount) * base.DynamicVars[KuugaDamageBonusKey].BaseValue;

            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue + kuugaDamageBonus)
                .WithHitCount(base.DynamicVars.Repeat.IntValue)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars[KuugaDamageBonusKey].UpgradeValueBy(1m);
        }
    }
}
