using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Enchantments;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiSharedRelicPool))]
    public sealed class MiracleGohei_Koishi : CustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Ancient;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new CardsVar(4)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<MiracleEnchantment>(2);

        public override string PackedIconPath => $"res://mods/Komeiji_Koishi/images/relics/MiracleGohei_Koishi.png";
        protected override string PackedIconOutlinePath => $"res://mods/Komeiji_Koishi/images/relics/MiracleGohei_Koishi.png";
        protected override string BigIconPath => $"res://mods/Komeiji_Koishi/images/relics/MiracleGohei_Koishi.png";

        public override async Task AfterObtained()
        {
            CardSelectorPrefs cardSelectorPrefs = new CardSelectorPrefs(
                CardSelectorPrefs.EnchantSelectionPrompt,
                base.DynamicVars.Cards.IntValue
            );

            IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromDeckForEnchantment(
                base.Owner,
                ModelDb.Enchantment<MiracleEnchantment>(),
                2,
                cardSelectorPrefs
            );

            foreach (CardModel cardModel in selectedCards)
            {
                CardCmd.Enchant<MiracleEnchantment>(cardModel, 2m);

                NCardEnchantVfx? enchantVfx = NCardEnchantVfx.Create(cardModel);
                if (enchantVfx != null)
                {
                    NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(enchantVfx);
                }
            }
        }
    }
}
