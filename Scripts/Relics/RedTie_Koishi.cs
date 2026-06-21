using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Cards.Danmaku;
using KomeijiKoishi.Enchantments;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace KomeijiKoishi.Relics
{
    [Pool(typeof(KoishiSharedRelicPool))]
    public sealed class RedTie_Koishi : CustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Ancient;

        protected override IEnumerable<IHoverTip> ExtraHoverTips
        {
            get
            {
                List<IHoverTip> tips = new List<IHoverTip>();
                tips.AddRange(HoverTipFactory.FromCardWithCardHoverTips<YinYangOrbDanmaku_Koishi>(false));
                tips.AddRange(HoverTipFactory.FromEnchantment<MiracleEnchantment>(2));
                return tips;
            }
        }

        public override string PackedIconPath => "res://mods/Komeiji_Koishi/images/relics/RedTie_Koishi.png";
        protected override string PackedIconOutlinePath => "res://mods/Komeiji_Koishi/images/relics/RedTie_Koishi.png";
        protected override string BigIconPath => "res://mods/Komeiji_Koishi/images/relics/RedTie_Koishi.png";

        public override async Task AfterObtained()
        {
            List<CardModel> attackCards = PileType.Deck.GetPile(base.Owner).Cards
                .Where(card => card.Type == CardType.Attack)
                .ToList();

            foreach (CardModel card in attackCards)
            {
                CardModel newCard = base.Owner.RunState.CreateCard<YinYangOrbDanmaku_Koishi>(base.Owner);
                if (card.IsUpgraded)
                {
                    CardCmd.Upgrade(newCard);
                }

                await CardCmd.Transform(card, newCard, CardPreviewStyle.GridLayout);
                CardCmd.Enchant<MiracleEnchantment>(newCard, 2m);
            }
        }
    }
}
