using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Models.CardPools;
using BaseLib.Utils;

namespace KomeijiKoishi.Cards.Fumo
{
    [Pool(typeof(TokenCardPool))]
    public sealed class FlandreFumo_Koishi : CustomCardModel
    {
        public FlandreFumo_Koishi() 
            : base(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/fumo/{GetType().Name}.png";
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DamageVar(30m, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move)
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target == null) return;
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_heavy.mp3")
                .Targeting(cardPlay.Target)
                .Execute(choiceContext);
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(+385); 
        }
    }
}