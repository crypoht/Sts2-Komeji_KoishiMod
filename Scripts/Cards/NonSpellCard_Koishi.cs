using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils; 
using BaseLib.Abstracts; 
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Factories; 
using MegaCrit.Sts2.Core.GameActions.Multiplayer; 
using MegaCrit.Sts2.Core.Nodes.CommonUi; 
using KomeijiKoishi.Pools; 
using KomeijiKoishi.Enums;
using KomeijiKoishi.Cards.Danmaku; 
using MegaCrit.Sts2.Core.HoverTips;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class NonSpellCard_Koishi : CustomCardModel
    {
        public NonSpellCard_Koishi() 
            : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true) { }

        public override string PortraitPath => $"res://mods/Komeiji_Koishi/images/cards/{GetType().Name}.png";

                protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] 
        { 
            HoverTipFactory.FromKeyword(KoishiKeywords.Unconscious) 
        };

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar> { new CardsVar(2) };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            try
            {
                await CreatureCmd.TriggerAnim(base.Owner!.Creature, "Cast", base.Owner.Character!.CastAnimDelay);

                IEnumerable<CardModel> generatedCards = CardFactory.GetForCombat(
                    base.Owner, 
                    DanmakuPool.Pool, 
                    base.DynamicVars.Cards.IntValue, 
                    base.Owner.RunState!.Rng!.CombatCardGeneration!
                );

                foreach (CardModel generatedDanmaku in generatedCards)
                {
                    CardPileAddResult result = await CardPileCmd.AddGeneratedCardToCombat(generatedDanmaku, PileType.Hand, true, CardPilePosition.Bottom);
                    CardCmd.PreviewCardPileAdd(result, 2.2f, CardPreviewStyle.HorizontalLayout);
                    await Cmd.Wait(0.1f, false);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[NonSpellCard] FATAL ERROR PREVENTED: {e}");
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Cards.UpgradeValueBy(1m);
        }
    }
}