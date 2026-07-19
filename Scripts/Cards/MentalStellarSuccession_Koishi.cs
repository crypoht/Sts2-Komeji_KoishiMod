using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using MegaCrit.Sts2.Core.HoverTips;
namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class MentalStellarSuccession_Koishi : CustomCardModel
    {
        public MentalStellarSuccession_Koishi()
            : base(2, CardType.Power, CardRarity.Rare, TargetType.Self, true) { }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new DynamicVar("Multiplier", KomeijiKoishi.Config.KoishiBalanceManager.Value(100m, 50m))
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new[]
        {
            HoverTipFactory.FromPower<TracingPower>()
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try
            {
                var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
                if (player == null) return;

                int multiplier = GetCurrentMultiplier();
                await PowerCmd.Apply<MentalStellarSuccessionPower>(
                    choiceContext,
                    player.Creature,
                    multiplier,
                    player.Creature,
                    this,
                    false
                );
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[MentalStellarSuccession_Koishi] Error: {e.Message}");
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars["Multiplier"].UpgradeValueBy(KomeijiKoishi.Config.KoishiBalanceManager.Value(50m, 25m));
        }

        private int GetCurrentMultiplier()
        {
            int baseValue = KomeijiKoishi.Config.KoishiBalanceManager.Value(100, 50);
            int upgradeValue = KomeijiKoishi.Config.KoishiBalanceManager.Value(50, 25);
            return baseValue + upgradeValue * CurrentUpgradeLevel;
        }
    }
}
