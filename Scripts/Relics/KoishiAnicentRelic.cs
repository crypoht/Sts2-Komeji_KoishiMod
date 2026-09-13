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
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics; 
using KomeijiKoishi.Pools;
using KomeijiKoishi.Enums; 
using KomeijiKoishi.Utils_Koishi; 
using MegaCrit.Sts2.Core.Entities.Relics; 
using MegaCrit.Sts2.Core.HoverTips; 
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiRelicPool))]
    public sealed class KoishiAnicentRelic : CustomRelicModel
    {
        private const decimal TeamDamageReflectionMultiplier = 10m;
        private decimal pendingTeamDamageReflection;

        public override RelicRarity Rarity => RelicRarity.Starter;

        public override string PackedIconPath => "res://mods/Komeiji_Koishi/images/relics/koishi_anicent_relic.png";
        protected override string PackedIconOutlinePath => "res://mods/Komeiji_Koishi/images/relics/koishi_anicent_relic.png";
        protected override string BigIconPath => "res://mods/Komeiji_Koishi/images/relics/koishi_anicent_relic.png";
        
        protected override IEnumerable<IHoverTip> ExtraHoverTips => new List<IHoverTip>
        {
            HoverTipFactory.FromPower<ThornsPower>()
        };

#if STS2_BETA
        public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
        {
            return ModifyDamageMultiplicativeCore(target, amount, dealer);
        }
#else
        public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            return ModifyDamageMultiplicativeCore(target, amount, dealer);
        }
#endif

        private decimal ModifyDamageMultiplicativeCore(Creature? target, decimal amount, Creature? dealer)
        {
            if (IsTeamDamageToOwner(target, dealer) && !KoishiExtensions.IsReflectingTeamDamage)
            {
                this.pendingTeamDamageReflection = amount;
                return 0m;
            }

            return 1m;
        }

        public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (!IsTeamDamageToOwner(target, dealer) || KoishiExtensions.IsReflectingTeamDamage || this.pendingTeamDamageReflection <= 0m)
            {
                return;
            }

            decimal reflectedDamage = this.pendingTeamDamageReflection * TeamDamageReflectionMultiplier;
            this.pendingTeamDamageReflection = 0m;
            base.Flash();

            KoishiExtensions.IsReflectingTeamDamage = true;
            try
            {
                await CreatureCmd.Damage(choiceContext, dealer!, reflectedDamage, ValueProp.Unblockable | ValueProp.Unpowered, base.Owner.Creature);
            }
            finally
            {
                KoishiExtensions.IsReflectingTeamDamage = false;
            }
        }

        private bool IsTeamDamageToOwner(Creature? target, Creature? dealer)
        {
            return target == base.Owner.Creature
                && dealer != null
                && dealer != base.Owner.Creature
                && dealer.Side == base.Owner.Creature.Side;
        }

        public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
        {
            if (cardPlay.Card.Owner.Creature != base.Owner.Creature) return;

            if (!KoishiExtensions.IsTrulyUnconscious(cardPlay.Card)) return;

            this.Flash(); 

            await PowerCmd.Apply<ThornsPower>(context,base.Owner.Creature, 1m, base.Owner.Creature, null, false);
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
                MegaCrit.Sts2.Core.Logging.Log.Error($"[Relic] KoishiAnicentRelic Error: {e.Message}");
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
                MegaCrit.Sts2.Core.Logging.Log.Error($"[Relic] KoishiAnicentRelic WineFox compat Error: {e.Message}");
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
