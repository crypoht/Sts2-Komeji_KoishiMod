using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Models.CardPools;
using BaseLib.Utils;

namespace KomeijiKoishi.Cards.Fumo
{
    [Pool(typeof(TokenCardPool))]
    public sealed class YoumuFumo_Koishi : CustomCardModel
    {
        public YoumuFumo_Koishi() 
            : base(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/fumo/{GetType().Name}.png";
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DamageVar(13m, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
            new RepeatVar(2)
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target == null) return;

            NGrandFinaleVfx? grandFinaleVfx = NGrandFinaleVfx.Create(base.Owner.Creature);
            if (grandFinaleVfx != null)
            {
                NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(grandFinaleVfx);
                await Cmd.Wait(NGrandFinaleVfx.totalAnticipationDuration, false);
            }

            AttackCommand attackCommand = DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                .WithHitCount(base.DynamicVars.Repeat.IntValue)
                .FromCard(this)
                .Targeting(cardPlay.Target);

            await attackCommand
                .WithHitVfxNode((Creature target) => NGrandFinaleImpactVfx.Create(target))
                .WithHitFx(null, null, "blunt_attack.mp3")
                .Execute(choiceContext);
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(+385); 
        }
    }
}
