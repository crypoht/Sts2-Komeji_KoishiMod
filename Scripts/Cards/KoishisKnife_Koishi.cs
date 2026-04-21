using System;
using System.Collections.Generic;
using System.Linq; 
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Pools;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models;  
using KomeijiKoishi.Enums;
using MegaCrit.Sts2.Core.HoverTips; 
using MegaCrit.Sts2.Core.Nodes.CommonUi; 
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Combat; 

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class KoishisKnife_Koishi : CustomCardModel
    {
        public KoishisKnife_Koishi() 
            : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        public override int MaxUpgradeLevel => 514114514;

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.Static(StaticHoverTip.Fatal, Array.Empty<DynamicVar>())
        };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DamageVar(3m, ValueProp.Move) 
        };

       protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            
            var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
            if (player == null) return; 

            if (player.Character != null)
            {
                await CreatureCmd.TriggerAnim(player.Creature, "Attack", player.Character.AttackAnimDelay);
            }

            bool shouldTriggerFatal = cardPlay.Target.Powers.All((PowerModel p) => p.ShouldOwnerDeathTriggerFatal());

            var attackCommand = await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash", null, "knife_attack.mp3") 
                .Execute(choiceContext);


           if (shouldTriggerFatal && attackCommand.Results.Any(r => r.WasTargetKilled))
            {

                CardCmd.Upgrade(this, CardPreviewStyle.HorizontalLayout);


                if (base.IsClone) return;


                var masterDeckPile = PileType.Deck.GetPile(player);
                if (masterDeckPile != null && masterDeckPile.Cards != null)
                {
                    var originalKnife = masterDeckPile.Cards.FirstOrDefault(c => c is KoishisKnife_Koishi);
                    if (originalKnife != null)
                    {

                        if (base.CombatState?.HittableEnemies?.All(e => e.IsDead) == true)
                        {

                            CardCmd.Upgrade(originalKnife, CardPreviewStyle.None);
                        }
                        else
                        {
                            CardCmd.Upgrade(originalKnife, CardPreviewStyle.HorizontalLayout);
                        }
                    }
                }
            }
        }

        protected override void OnUpgrade()
        {
            decimal rawIncrease = base.DynamicVars.Damage.BaseValue * 0.4m;
            
            decimal finalIncrease = Math.Floor(rawIncrease);
            if (finalIncrease < 1m) 
            {
                finalIncrease = 1m;
            }
            base.DynamicVars.Damage.UpgradeValueBy(finalIncrease);
        }
    }
}