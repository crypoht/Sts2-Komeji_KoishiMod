using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat; 
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players; 
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models.Powers; 
using KomeijiKoishi.Enums;
using KomeijiKoishi.Pools;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))] 
    public sealed class RoseDanmaku_Koishi : CustomCardModel
    {
        public RoseDanmaku_Koishi() 
            : base(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy, false) 
        { 
        }
        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { KoishiTags.Danmaku };
        
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DamageVar(10m, ValueProp.Move),
            new DynamicVar("Thorns", 1m)       
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try
            {
                var player = base.Owner as Player;
                if (player == null || cardPlay.Target == null) return;

                await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                    .FromCard(this)
                    .Targeting(cardPlay.Target)
                    .WithHitFx("vfx/vfx_attack_slash")
                    .Execute(choiceContext);

                await PowerCmd.Apply<ThornsPower>(player.Creature, base.DynamicVars["Thorns"].BaseValue, player.Creature, this, false);
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[RoseDanmaku_Koishi] Error: {e.Message}");
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Damage.UpgradeValueBy(4m);
        }

        public static async Task CreateInHand(Player owner, int count, CombatState combatState)
        {
            if (count <= 0 || CombatManager.Instance.IsOverOrEnding) return;

            List<CardModel> roses = new List<CardModel>();
            for (int i = 0; i < count; i++)
            {
                roses.Add(combatState.CreateCard<RoseDanmaku_Koishi>(owner));
            }
            
            await CardPileCmd.AddGeneratedCardsToCombat(roses, PileType.Hand, true, CardPilePosition.Bottom);
        }
    }
}