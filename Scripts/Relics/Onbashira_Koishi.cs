using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Cards;
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
using MegaCrit.Sts2.Core.Models.Relics;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiSharedRelicPool))]
    public sealed class Onbashira_Koishi : CustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Ancient;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new CardsVar(1)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
            HoverTipFactory.FromCardWithCardHoverTips<MishagujiPillar_Koishi>(false);

        public override string PackedIconPath => $"res://mods/Komeiji_Koishi/images/relics/Onbashira_Koishi.png";
        protected override string PackedIconOutlinePath => $"res://mods/Komeiji_Koishi/images/relics/Onbashira_Koishi.png";
        protected override string BigIconPath => $"res://mods/Komeiji_Koishi/images/relics/Onbashira_Koishi.png";

        public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
        {
            if (player != base.Owner || combatState.RoundNumber != 1)
            {
                return;
            }

            base.Flash();

            List<CardModel> cards = new List<CardModel>();
            for (int i = 0; i < base.DynamicVars.Cards.IntValue; i++)
            {
                cards.Add(combatState.CreateCard<MishagujiPillar_Koishi>(player));
            }

            await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Draw, player, CardPilePosition.Random);
        }
    }
}
