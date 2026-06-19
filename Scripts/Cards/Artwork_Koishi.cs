using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using BaseLib.Abstracts; 
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;  
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Pools; 

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class Artwork_Koishi : CustomCardModel
    {
        public Artwork_Koishi()
            : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
        {
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";
        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DamageVar(6m, ValueProp.Move),
            new BlockVar(6m, ValueProp.Move) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target != null)
            {
                await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                    .FromCard(this)
                    .Targeting(cardPlay.Target)
                    .WithHitFx("vfx/vfx_attack_slash", null, null)
                    .Execute(choiceContext);
            }

            foreach (var kvp in base.DynamicVars)
            {
                if (kvp.Value is BlockVar bv)
                {
                    await CreatureCmd.GainBlock(base.Owner.Creature, bv, cardPlay, false);
                    break; 
                }
            }
        }

        protected override void OnUpgrade()
        {
            foreach (var kvp in base.DynamicVars)
            {
                if (kvp.Value is DamageVar dv) dv.UpgradeValueBy(2m);
                if (kvp.Value is BlockVar bv) bv.UpgradeValueBy(2m);
            }
        }
    }
}