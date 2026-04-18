using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Enums;
using KomeijiKoishi.Utils_Koishi;
using KomeijiKoishi.Cards.Danmaku; 
using MegaCrit.Sts2.Core.Entities.Powers; 
using MegaCrit.Sts2.Core.Models; 
using MegaCrit.Sts2.Core.Models.Cards; 
using System;
using System.Linq;
using BaseLib.Abstracts;

namespace KomeijiKoishi.Powers
{
    public sealed class MiraclePower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override string? CustomPackedIconPath => $"res://mods/Komeiji_Koishi/images/powers/MiraclePower.png";
        public override string? CustomBigIconPath => $"res://mods/Komeiji_Koishi/images/powers/MiraclePower.png";

        private bool _isDuplicating = false;

        public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
        {
            if (base.CombatState == null) return;

            if (_isDuplicating) return;

            if (cardPlay.Card.Tags != null && cardPlay.Card.Tags.Contains(KoishiTags.Danmaku))
            {
                this.Flash();
                
                int timesToPlay = (int)base.Amount;

                try 
                {
                    _isDuplicating = true;

                    for (int i = 0; i < timesToPlay; i++)
                    {
                        if (base.CombatState.HittableEnemies.All(e => e.IsDead)) break;

                        await CardCmd.AutoPlay(
                            context, 
                            cardPlay.Card, 
                            cardPlay.Target, 
                            AutoPlayType.Default, 
                            true, 
                            false 
                        );
                    }
                }
                finally 
                {
                    _isDuplicating = false;
                }
            }
        }
    }
}