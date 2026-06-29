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
using KomeijiKoishi.Cards;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiRelicPool))]
    public sealed class KoishiStarterRelic : CustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        public override string PackedIconPath => $"res://mods/Komeiji_Koishi/images/relics/{Id.Entry.ToLowerInvariant()}.png";
        protected override string PackedIconOutlinePath => $"res://mods/Komeiji_Koishi/images/relics/{Id.Entry.ToLowerInvariant()}.png";

        protected override string BigIconPath => $"res://mods/Komeiji_Koishi/images/relics/{Id.Entry.ToLowerInvariant()}.png";
        public override RelicModel? GetUpgradeReplacement() => ModelDb.Relic<KoishiAnicentRelic>();

        public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
        {
            if (card is CompleteUnconscious_Koishi)
            {
                modifiedCost = 514m;
                return true; 
            }
            
            modifiedCost = originalCost;
            return false;
        }

       public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            try
            {
                if (side != base.Owner.Creature.Side) return;

                var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
                if (player == null) return;

                if (KoishiExtensions.HasWineFoxPlanningExpertInTeam(player)) return;

                await AutoPlayUnconsciousCard(choiceContext, player);
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[Relic] KoishiStarterRelic Error: {e.Message}");
            }
        }

        public override async Task BeforeFlushLate(PlayerChoiceContext choiceContext, Player player)
        {
            try
            {
                if (player != base.Owner) return;
                if (!KoishiExtensions.HasWineFoxPlanningExpertInTeam(player)) return;

                await AutoPlayUnconsciousCard(choiceContext, player);
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[Relic] KoishiStarterRelic WineFox compat Error: {e.Message}");
            }
        }

        private async Task AutoPlayUnconsciousCard(PlayerChoiceContext choiceContext, Player player)
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
                        this.Flash(); 

                        await KoishiExtensions.SafeAutoPlayCard(
                            choiceContext, 
                            player, 
                            targetCard  
                        );
                    }
                }
        }
    }
}
