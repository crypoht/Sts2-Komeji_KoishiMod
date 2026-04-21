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
using KomeijiKoishi.Utils_Koishi; 
using System.Linq;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.HoverTips;
using KomeijiKoishi.Enums;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class AirStrike_Koishi : CustomCardModel
    {
        public AirStrike_Koishi() 
            : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, true) 
        { 
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DamageVar(7m, ValueProp.Move) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target == null) return;

            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_blunt") 
                .Execute(choiceContext);
        }

       public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Card.Owner == base.Owner)
            {
                if (cardPlay.Card != this && KoishiExtensions.IsTrulyUnconscious(cardPlay.Card))
                {
                    if (!MegaCrit.Sts2.Core.Combat.CombatManager.Instance.IsInProgress)
                    {
                        return; 
                    }

                    var pile = base.Pile;
                    if (pile != null && pile.Type != PileType.Exhaust && pile.Type != PileType.Hand)
                    {
                        await Cmd.Wait(0.05f, true);
                        if (base.Pile != null && base.Pile.Type != PileType.Exhaust && base.Pile.Type != PileType.Hand)
                        {
                            await CardPileCmd.Add(this, PileType.Hand, CardPilePosition.Bottom, null, false);
                        }
                    }
                }
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Damage.UpgradeValueBy(4m);
        }
    }
}