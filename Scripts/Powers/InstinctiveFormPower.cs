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


        public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
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
    }
}
