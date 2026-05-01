using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Pools;
using BaseLib.Utils;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class MalletLoveSign_Koishi : CustomCardModel
    {
        public MalletLoveSign_Koishi() 
            : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true) 
        { 
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DamageVar(1m, ValueProp.Move),
            new RepeatVar(3),
            new EnergyVar(1) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as Player;
            if (player == null || cardPlay.Target == null) return;

  
            var attackCommandResult = await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                .WithHitCount(base.DynamicVars.Repeat.IntValue) 
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_blunt", null, null) 
                .Execute(choiceContext);

            int energyToGain = attackCommandResult.Results.Count(r => r.UnblockedDamage > 0);

            if (energyToGain > 0)
            {
                await PlayerCmd.GainEnergy(energyToGain, player);
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Repeat.UpgradeValueBy(1m);
        }
    }
}