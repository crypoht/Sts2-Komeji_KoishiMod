using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Enums;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class HardmansYoukaiGirl_Koishi : CustomCardModel
    {
        public HardmansYoukaiGirl_Koishi()
            : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly, true)
        {
        }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

        public override TargetType TargetType => base.IsUpgraded ? TargetType.AllAllies : TargetType.AnyAlly;

         protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromKeyword(KoishiKeywords.Danmaku),
        };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new PowerVar<HardmansYoukaiGirlPower>(1m)
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (base.CombatState == null)
            {
                return;
            }

            await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

            IEnumerable<Creature> targets;
            if (base.IsUpgraded)
            {
                targets = base.CombatState.Players
                    .Where(p => p != null && p != base.Owner && p.Creature != null && !p.Creature.IsDead && p.Creature.Side == base.Owner.Creature.Side)
                    .Select(p => p.Creature);
            }
            else
            {
                targets = cardPlay.Target != null ? new[] { cardPlay.Target } : Enumerable.Empty<Creature>();
            }

            foreach (Creature target in targets)
            {
                await PowerCmd.Apply<HardmansYoukaiGirlPower>(
                    choiceContext,
                    target,
                    base.DynamicVars["HardmansYoukaiGirlPower"].BaseValue,
                    base.Owner.Creature,
                    this,
                    false);
            }
        }
    }
}
