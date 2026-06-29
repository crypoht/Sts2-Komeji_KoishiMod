using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using KomeijiKoishi.Enums;
using MegaCrit.Sts2.Core.HoverTips;
using BaseLib.Utils;
using KomeijiKoishi.Cards.Danmaku; 

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class EmergencySpellCard_Koishi : CustomCardModel
    {
        public EmergencySpellCard_Koishi() 
            : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true) { }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> 
        { 

            new CardsVar(4),
            new PowerVar<KuugaPower>(2m)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromKeyword(KoishiKeywords.Danmaku),
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try
            {
                var player = base.Owner as Player;
                
                var combatState = base.CombatState as MegaCrit.Sts2.Core.Combat.CombatState; 
                
                if (player == null || combatState == null) return;

                await CreatureCmd.TriggerAnim(player.Creature, "Cast", player.Character!.CastAnimDelay);

                int count = base.DynamicVars.Cards.IntValue;
                for (int i = 0; i < count; i++)
                {
                    // 👇 传入转换好的 combatState
                    await DanmakuPool.CreateRandomDanmakuInHand(player, combatState, this);
                    await Cmd.Wait(0.1f, false);
                }

                await PowerCmd.Apply<KuugaPower>(
                    choiceContext,
                    player.Creature, 
                    -base.DynamicVars["KuugaPower"].BaseValue, 
                    player.Creature, 
                    this, 
                    false
                );
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[EmergencySpellCard_Koishi] Error: {e.Message}");
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Cards.UpgradeValueBy(1m);
        }
    }
}
