using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class RandomStrike_Koishi : CustomCardModel
    {
        public RandomStrike_Koishi()
            : base(0, CardType.Attack, CardRarity.Common, TargetType.RandomEnemy, true)
        {
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new DamageVar(4m, ValueProp.Move),
            new RepeatVar(2)
        };

        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (base.CombatState == null)
            {
                return;
            }

            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                .WithHitCount(base.DynamicVars.Repeat.IntValue)
                .FromCard(this)
                .TargetingRandomOpponents(base.CombatState, true)
                .WithHitFx("vfx/vfx_attack_slash", null, null)
                .Execute(choiceContext);
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Damage.UpgradeValueBy(1m);
        }
    }
}
