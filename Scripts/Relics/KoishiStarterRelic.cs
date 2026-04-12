using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Creatures; 
using MegaCrit.Sts2.Core.Entities.Relics; 
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics; 
using KomeijiKoishi.Pools;
using KomeijiKoishi.Enums; 
using KomeijiKoishi.Utils_Koishi; 

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiRelicPool))]
    public sealed class KoishiStarterRelic : CustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override string PackedIconPath => $"res://mods/Komeiji_Koishi/images/relics/{Id.Entry.ToLowerInvariant()}.png";
        protected override string PackedIconOutlinePath => $"res://mods/Komeiji_Koishi/images/relics/outline/{Id.Entry.ToLowerInvariant()}.png";

        public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
        {
            try
            {
                if (side != base.Owner.Creature.Side)
                {
                    return;
                }

                var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
                if (player == null) return;

                var list = PileType.Hand.GetPile(player).Cards.Where(c => 
                    KoishiExtensions.IsTrulyUnconscious(c) && 
                    (c.Keywords == null || !c.Keywords.Contains(CardKeyword.Unplayable))
                ).ToList();

                if (list.Count > 0)
                {
                    var targetCard = player.RunState.Rng.Shuffle.NextItem<CardModel>(list);

                    if (targetCard != null)
                    {
                        this.Flash(); 

                        Creature? targetCreature = null;
                        var combatState = player.Creature.CombatState; 
                        
                        if (combatState != null)
                        {
                            var validEnemies = combatState.HittableEnemies.Where(e => !e.IsDead).ToList();
                            if (validEnemies.Count > 0)
                            {
                                targetCreature = player.RunState.Rng.Shuffle.NextItem(validEnemies);
                            }
                        }

                        await CardCmd.AutoPlay(choiceContext, targetCard, targetCreature, AutoPlayType.Default, true, false);
                    }
                }
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[Relic] KoishiStarterRelic Error: {e.Message}");
            }
        }
    }
}