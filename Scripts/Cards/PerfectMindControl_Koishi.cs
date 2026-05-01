using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using BaseLib.Utils;
using KomeijiKoishi.Enums;
using MegaCrit.Sts2.Core.HoverTips;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class PerfectMindControl_Koishi : CustomCardModel
    {
        public PerfectMindControl_Koishi() 
            : base(-1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

        protected override bool HasEnergyCostX => true;

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromKeyword(KoishiKeywords.Unconscious) 
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner as Player;
            if (player == null) return;

            await CreatureCmd.TriggerAnim(player.Creature, "Cast", player.Character.CastAnimDelay);

            int xValue = base.ResolveEnergyXValue();


            int amount = xValue + (base.IsUpgraded ? 1 : 0);

            if (amount > 0)
            {
                await PowerCmd.Apply<PerfectMindControlPower>(
                    player.Creature, 
                    amount, 
                    player.Creature, 
                    this
                );
            }
        }

        protected override void OnUpgrade()
        {
        }
    }
}