using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models; // 🌟 引入 PileType
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Models.CardPools;
using System;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Gold; // 🌟 引入金币相关枚举

namespace KomeijiKoishi.Cards.Fumo
{
    [Pool(typeof(TokenCardPool))]
    public sealed class ReimuFumo_Koishi : CustomCardModel
    {
        public ReimuFumo_Koishi() 
            : base(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy, true) 
        { 
        }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/fumo/{GetType().Name}.png";
        
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 
            new DamageVar(50m, ValueProp.Move), 
            new RepeatVar(10)                   
        };


        protected override bool ShouldGlowGoldInternal => this.IsPlayable;

        protected override bool IsPlayable
        {
            get
            {
                var player = base.Owner as Player;
                if (player == null) return false;
                

                return player.Gold >= 100;
            }
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as Player;

            if (player == null || cardPlay.Target == null) return;

  
            await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
                    .WithHitCount(base.DynamicVars.Repeat.IntValue)
                    .FromCard(this)
                    .Targeting(cardPlay.Target)
                    .WithHitFx("vfx/vfx_attack_blunt") 
                    .Execute(choiceContext);
  
            try
            {
                await PlayerCmd.LoseGold(100, player, GoldLossType.Spent);
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[ReimuFumo] 扣除金币失败: {e.Message}");
            }
        }

        protected override PileType GetResultPileTypeForCardPlay()
        {
            PileType resultPileType = base.GetResultPileTypeForCardPlay();
            
            if (resultPileType != PileType.Discard)
            {
                return resultPileType;
            }
            return PileType.Hand;
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(+385); 
        }
    }
}