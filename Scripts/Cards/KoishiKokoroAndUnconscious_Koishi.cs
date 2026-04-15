using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Powers;
using KomeijiKoishi.Pools;
using System; 
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models;  
using KomeijiKoishi.Enums; 

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class KoishiKokoroAndUnconscious_Koishi : CustomCardModel
    {
        public KoishiKokoroAndUnconscious_Koishi()
            : base(3, CardType.Power, CardRarity.Ancient, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

         protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { KoishiTags.Subconscious };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[]
        {
            HoverTipFactory.FromPower<StrengthPower>(),
            HoverTipFactory.FromPower<DexterityPower>()
        };

protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new PowerVar<JoyMaskPower>(1m),
            new PowerVar<StrengthPower>(2m),
            new PowerVar<DexterityPower>(2m), 
            new CardsVar(1)
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<KoishiKokoroPower>(base.Owner.Creature, 1m, base.Owner.Creature, this, false);
        }

        protected override void OnUpgrade()
        {
            foreach (var variable in base.DynamicVars)
            {
                variable.Value.UpgradeValueBy(1m);
            }
        }
    }
}