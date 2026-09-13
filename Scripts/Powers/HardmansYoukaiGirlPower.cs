using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using KomeijiKoishi.Cards.Danmaku;
using KomeijiKoishi.Utils_Koishi;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace KomeijiKoishi.Powers
{
    public sealed class HardmansYoukaiGirlPower : CustomPowerModel
    {
        private bool _isPlayingDanmaku;

        [SavedProperty]
        public bool GenerateUpgradedDanmaku { get; set; }

        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override string? CustomPackedIconPath => "res://mods/Komeiji_Koishi/images/powers/HardmansYoukaiGirlPower.png";
        public override string? CustomBigIconPath => "res://mods/Komeiji_Koishi/images/powers/HardmansYoukaiGirlPower.png";

        public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (_isPlayingDanmaku || cardPlay.Card.Owner != base.Owner.Player || cardPlay.Card.Type != CardType.Attack)
            {
                return;
            }

            if (base.Owner.Player == null || base.Owner.IsDead || base.Owner.CombatState is not CombatState combatState)
            {
                return;
            }

            var aliveEnemies = combatState.HittableEnemies.Where(e => e != null && !e.IsDead).ToList();
            if (aliveEnemies.Count == 0)
            {
                return;
            }

            base.Flash();
            int times = (int)base.Amount;

            try
            {
                _isPlayingDanmaku = true;
                for (int i = 0; i < times; i++)
                {
                    CardModel? danmaku = await DanmakuPool.CreateRandomDanmakuInExhaust(base.Owner.Player, combatState, cardPlay.Card);
                    if (danmaku == null)
                    {
                        continue;
                    }

                    if (GenerateUpgradedDanmaku)
                    {
                        CardCmd.Upgrade(danmaku, CardPreviewStyle.None);
                    }

                    Creature? target = null;
                    if (danmaku.TargetType == TargetType.AnyEnemy)
                    {
                        target = base.Owner.Player.RunState.Rng.Shuffle.NextItem(aliveEnemies);
                    }

                    await KoishiExtensions.SafeAutoPlayCard(choiceContext, base.Owner.Player, danmaku, target, AutoPlayType.Default, true, false);
                }
            }
            finally
            {
                _isPlayingDanmaku = false;
            }
        }

        public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (participants.Contains(base.Owner))
            {
                await PowerCmd.Remove(this);
            }
        }
    }
}
