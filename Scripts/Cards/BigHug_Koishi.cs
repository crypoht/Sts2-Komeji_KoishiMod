using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Enums;
using KomeijiKoishi.Utils_Koishi;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class BigHug_Koishi : CustomCardModel
    {
        public BigHug_Koishi()
            : base(3, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
        {
        }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new List<IHoverTip>
        {
            HoverTipFactory.FromKeyword(KoishiKeywords.Unconscious)
        };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new DamageVar(31m, ValueProp.Move),
            new DynamicVar("Growth", 9m)
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
                
                await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                    .FromCard(this, cardPlay)
                    .Targeting(cardPlay.Target)
                    .WithHitFx("vfx/vfx_attack_blunt") 
                    .Execute(choiceContext);
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[BigHug_Koishi] OnPlay Error: {e.Message}");
            }
        }

        public override Task AfterCardEnteredCombat(CardModel card)
        {
            if (card != this || base.IsClone || base.CombatState == null) return Task.CompletedTask;

            int playedCount = CombatManager.Instance.History.CardPlaysFinished.Count((CardPlayFinishedEntry e) =>
                e.CardPlay.Card.Owner == base.Owner &&
                KoishiExtensions.IsTrulyUnconscious(e.CardPlay.Card));

            if (playedCount > 0)
            {
                base.DynamicVars.Damage.BaseValue += playedCount * base.DynamicVars["Growth"].BaseValue;
            }

            return Task.CompletedTask;
        }

        public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
        {
            if (cardPlay.Card.Owner == base.Owner && KoishiExtensions.IsTrulyUnconscious(cardPlay.Card))
            {
                base.DynamicVars.Damage.BaseValue += base.DynamicVars["Growth"].BaseValue;
            }

            return Task.CompletedTask;
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Damage.UpgradeValueBy(10m);
            base.DynamicVars["Growth"].UpgradeValueBy(4m);
        }
    }
}
