using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiSharedRelicPool))]
    public sealed class ExorcismAmulet_Koishi : CustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Ancient;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new CardsVar(1)
        };

        public override string PackedIconPath => "res://mods/Komeiji_Koishi/images/relics/ExorcismAmulet_Koishi.png";
        protected override string PackedIconOutlinePath => "res://mods/Komeiji_Koishi/images/relics/ExorcismAmulet_Koishi.png";
        protected override string BigIconPath => "res://mods/Komeiji_Koishi/images/relics/ExorcismAmulet_Koishi.png";

        public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
        {
            if (card.Owner != base.Owner)
            {
                return;
            }

            if (card.Type != CardType.Status && card.Type != CardType.Curse)
            {
                return;
            }

            base.Flash();
            await CardCmd.Exhaust(choiceContext, card, false, false);
            await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner, false);
        }
    }
}
