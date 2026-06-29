using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers; 
using MegaCrit.Sts2.Core.Models.CardPools;
using BaseLib.Utils;

namespace KomeijiKoishi.Cards.Fumo
{
    [Pool(typeof(TokenCardPool))]
    public sealed class MarisaFumo_Koishi : CustomCardModel
    {
  
        public MarisaFumo_Koishi() 
            : base(0, CardType.Attack, CardRarity.Token, TargetType.AllEnemies, true) 
        { 
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/fumo/{GetType().Name}.png";
        
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DamageVar(70m, ValueProp.Move),
            new PowerVar<KuugaPower>(10m) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as Player;
            if (player == null || base.CombatState == null) return;


            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .TargetingAllOpponents(base.CombatState)
                .WithAttackerAnim("Cast", 0.5f, null)
                .BeforeDamage(async delegate
                {
                    List<Creature> enemies = base.CombatState.Enemies.Where(e => e.IsAlive).ToList();
                    if (enemies.Count == 0) return;

                    NHyperbeamVfx? hyperbeamVfx = NHyperbeamVfx.Create(base.Owner.Creature, enemies.Last());
                    if (hyperbeamVfx != null)
                    {
                        NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(hyperbeamVfx);
                        await Cmd.Wait(0.5f, false);
                    }

                    foreach (Creature enemy in enemies)
                    {
                        NHyperbeamImpactVfx? impactVfx = NHyperbeamImpactVfx.Create(base.Owner.Creature, enemy);
                        if (impactVfx != null)
                        {
                            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(impactVfx);
                        }
                    }
                })
                .Execute(choiceContext);

            await PowerCmd.Apply<KuugaPower>(
                choiceContext,
                player.Creature, 
                -base.DynamicVars["KuugaPower"].BaseValue, 
                player.Creature, 
                this, 
                false
            );
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(+385); 
        }
    }
}
