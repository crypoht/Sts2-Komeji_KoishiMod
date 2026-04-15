using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models.Cards;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Enums;
using KomeijiKoishi.Utils_Koishi;
using MegaCrit.Sts2.Core.Entities.Players;
using KomeijiKoishi.Powers;
using MegaCrit.Sts2.Core.Models;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class InstinctStrike_Koishi : CustomCardModel
    {
        public InstinctStrike_Koishi()
            : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
        {
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new List<IHoverTip>
        {
            HoverTipFactory.FromKeyword(KoishiKeywords.Unconscious)
        };

        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new DamageVar(8m, ValueProp.Move)
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash", null, null)
                .Execute(choiceContext);

            CardSelectorPrefs cardSelectorPrefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1);

            IEnumerable<CardModel> enumerable = await CardSelectCmd.FromHand(
                choiceContext, 
                base.Owner, 
                cardSelectorPrefs, 
                (CardModel c) => !KoishiExtensions.IsTrulyUnconscious(c), 
                this
            );

            var targetCard = enumerable.FirstOrDefault();
            
            if (targetCard != null)
            {
                KoishiExtensions.ApplyUnconsciousToCard(targetCard);
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Damage.UpgradeValueBy(5m);
        }
    }
}