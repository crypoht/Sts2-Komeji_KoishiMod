using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiSharedRelicPool))]
    public sealed class SuwakosHat_Koishi : CustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Ancient;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new CardsVar(1)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new List<IHoverTip>
        {
            HoverTipFactory.FromPower<ConfusedPower>()
        };

        public override string PackedIconPath => $"res://mods/Komeiji_Koishi/images/relics/SuwakosHat_Koishi.png";
        protected override string PackedIconOutlinePath => $"res://mods/Komeiji_Koishi/images/relics/SuwakosHat_Koishi.png";
        protected override string BigIconPath => $"res://mods/Komeiji_Koishi/images/relics/SuwakosHat_Koishi.png";

        public override async Task AfterObtained()
        {
            if (CombatManager.Instance.IsInProgress)
            {
                await this.ApplyPowers();
            }
        }

        public override async Task BeforeCombatStart()
        {
            await this.ApplyPowers();
        }

        public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
        {
            if (player != base.Owner)
            {
                return;
            }

            base.Flash();

            await CardPileCmd.ShuffleIfNecessary(choiceContext, player);

            IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromCombatPile(
                choiceContext,
                PileType.Draw.GetPile(player),
                player,
                new CardSelectorPrefs(base.SelectionScreenPrompt, base.DynamicVars.Cards.IntValue)
            );

            await CardPileCmd.Add(selectedCards, PileType.Hand, CardPilePosition.Bottom, null, false);
        }

        private async Task ApplyPowers()
        {
            base.Flash();

            await PowerCmd.Apply<ConfusedPower>(
                new ThrowingPlayerChoiceContext(),
                base.Owner.Creature,
                1m,
                base.Owner.Creature,
                null,
                false
            );
        }
    }
}
