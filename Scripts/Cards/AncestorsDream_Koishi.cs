using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts; 
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer; 
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using KomeijiKoishi.Pools; 
using BaseLib.Utils;
namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class AncestorsDream_Koishi : CustomCardModel
    {
        private const int MaxPreviewCards = 20;

        public AncestorsDream_Koishi() 
            : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true) { }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try
            {
                var player = base.Owner as MegaCrit.Sts2.Core.Entities.Players.Player;
                if (player == null) return;

                await CreatureCmd.TriggerAnim(player.Creature, "Cast", player.Character!.CastAnimDelay);

                var exhaustPile = PileType.Exhaust.GetPile(player);
                if (exhaustPile == null || !exhaustPile.Cards.Any()) return;

                List<CardModel> snapshot = exhaustPile.Cards.ToList();
                List<CardPileAddResult> addResults = new List<CardPileAddResult>();

                foreach (CardModel originalCard in snapshot)
                {
                    CardModel duplicatedCard = originalCard.CreateClone();

                    CardPileAddResult result = await CardPileCmd.AddGeneratedCardToCombat(
                        duplicatedCard, 
                        PileType.Exhaust, 
                        player, 
                        CardPilePosition.Bottom
                    );

                    addResults.Add(result);
                }

                if (addResults.Count > 0)
                {
                    CardCmd.PreviewCardPileAdd(addResults.Take(MaxPreviewCards).ToList(), 2.2f, CardPreviewStyle.HorizontalLayout);
                }
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[AncestorsDream] 运行异常: {e.Message}");
            }
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(-1); 
        }
    }
}
