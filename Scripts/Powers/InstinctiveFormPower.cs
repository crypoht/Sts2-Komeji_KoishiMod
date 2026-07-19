using System;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using KomeijiKoishi.Utils_Koishi; 
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Localization;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Powers;
using KomeijiKoishi.Enums;


namespace KomeijiKoishi.Powers
{
    public sealed class InstinctiveFormPower : CustomPowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        
        public override string? CustomPackedIconPath => $"res://mods/Komeiji_Koishi/images/powers/InstinctiveFormPower.png";
        public override string? CustomBigIconPath => $"res://mods/Komeiji_Koishi/images/powers/InstinctiveFormPower.png";

        protected override object InitInternalData() => new InstinctiveFormData();

        public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
        {
            if (!KomeijiKoishi.Config.KoishiBalanceManager.IsEnabled)
            {
                return;
            }

            if (cardPlay.Card.Owner != base.Owner.Player || !KoishiExtensions.IsTrulyUnconscious(cardPlay.Card))
            {
                return;
            }

            InstinctiveFormData data = base.GetInternalData<InstinctiveFormData>();
            data.UnconsciousPlayed++;
            if (data.UnconsciousPlayed < 2)
            {
                return;
            }

            data.UnconsciousPlayed -= 2;
            base.Flash();
            await CardPileCmd.Draw(context, 1, base.Owner.Player, false);
        }

        public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (KomeijiKoishi.Config.KoishiBalanceManager.IsEnabled)
            {
                return Task.CompletedTask;
            }

            try
            {
                if (player != base.Owner.Player) return Task.CompletedTask;

                int giveCount = (int)base.Amount;
                for (int i = 0; i < giveCount; i++)
                {
                    var candidates = PileType.Hand.GetPile(player).Cards.Where(c =>
                        !KoishiExtensions.IsTrulyUnconscious(c)
                    ).ToList();

                    if (candidates.Count <= 0) break;

                    var cardToMark = player.RunState.Rng.Shuffle.NextItem<CardModel>(candidates);
                    if (cardToMark == null) break;

                    base.Flash();
                    KoishiExtensions.ApplyUnconsciousToCard(cardToMark);
                }
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[Power] InstinctiveFormPower Start Error: {e.Message}");
            }

            return Task.CompletedTask;
        }

        public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (KomeijiKoishi.Config.KoishiBalanceManager.IsEnabled)
            {
                return;
            }

            try
            {

                if (side != base.Owner.Side) return;

                var player = base.Owner.Player;
                if (player == null) return;

                int playCount = (int)base.Amount;

                for (int i = 0; i < playCount; i++)
                {
                    var list = PileType.Hand.GetPile(player).Cards.Where(c => 
                        KoishiExtensions.IsTrulyUnconscious(c) && 
                        (c.Keywords == null || !c.Keywords.Contains(CardKeyword.Unplayable))
                    ).ToList();

                    if (list.Count > 0)
                    {
                        var targetCard = player.RunState.Rng.Shuffle.NextItem<CardModel>(list);

                        if (targetCard != null)
                        {
                            base.Flash(); 

                            await KoishiExtensions.SafeAutoPlayCard(choiceContext, player, targetCard);

                            await Cmd.Wait(0.2f, false);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[Power] InstinctiveFormPower Error: {e.Message}");
            }
        }

        private class InstinctiveFormData
        {
            public int UnconsciousPlayed;
        }
    }
}
