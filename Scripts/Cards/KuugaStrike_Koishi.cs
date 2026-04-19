using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using KomeijiKoishi.Cards.Danmaku;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class KuugaStrike_Koishi : CustomCardModel, IStanceListenerCard
    {
        public KuugaStrike_Koishi() 
            : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true) { }

        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        private class KuugaVar : DynamicVar { public KuugaVar(decimal val) : base("Kuuga", val) { } }

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DamageVar(1m, ValueProp.Move),
            new KuugaVar(1m) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target == null) return;
            var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
            if (player == null) return;

            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(choiceContext);

            decimal amount = base.DynamicVars["Kuuga"].BaseValue;
            await PowerCmd.Apply<KuugaPower>(player.Creature, amount, player.Creature, this, false);
        }

        public async Task OnStanceChanged(bool isClosedStance, bool isBloomStance)
        {
            var pile = base.Pile;
            
            if (pile != null && pile.Type != PileType.Hand && pile.Type != PileType.Exhaust)
            {
                await CardPileCmd.Add(this, PileType.Hand, CardPilePosition.Bottom, null, false);
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Damage.UpgradeValueBy(2m);
            base.DynamicVars["Kuuga"].UpgradeValueBy(1m);
        }
    }
}