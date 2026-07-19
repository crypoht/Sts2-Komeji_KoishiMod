using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi; 
using KomeijiKoishi.Pools;
using KomeijiKoishi.Powers;
using BaseLib.Utils;

namespace KomeijiKoishi.Cards
{
    [Pool(typeof(KoishiCardPool))]
    public sealed class IdsShikigami_Koishi : CustomCardModel
    {
        public IdsShikigami_Koishi() 
            : base(
                KomeijiKoishi.Config.KoishiBalanceManager.Value(3, 514),
                KomeijiKoishi.Config.KoishiBalanceManager.IsEnabled ? CardType.Skill : CardType.Power,
                CardRarity.Rare,
                TargetType.Self,
                true)
        {
        }

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        protected override bool HasEnergyCostX => KomeijiKoishi.Config.KoishiBalanceManager.IsEnabled;

        public override IEnumerable<CardKeyword> CanonicalKeywords =>
            KomeijiKoishi.Config.KoishiBalanceManager.IsEnabled
                ? new[] { CardKeyword.Exhaust }
                : Array.Empty<CardKeyword>();

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = base.Owner;
            if (player == null || base.CombatState == null) return;

            try
            {
                if (KomeijiKoishi.Config.KoishiBalanceManager.IsEnabled)
                {
                    await CreatureCmd.TriggerAnim(player.Creature, "Cast", player.Character.CastAnimDelay);

                    int xValue = base.ResolveEnergyXValue();
                    int generateCount = xValue * 2;
                    
                    if (generateCount <= 0) return;

                    var skillPool = from c in player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
                                    where c.Type == CardType.Skill
                                    select c;

                    for (int i = 0; i < generateCount; i++)
                    {
                        var randomSkill = CardFactory.GetDistinctForCombat(
                            player, 
                            skillPool, 
                            1, 
                            player.RunState.Rng.CombatCardGeneration
                        ).FirstOrDefault();
                        
                        if (randomSkill != null)
                        {
                            if (base.IsUpgraded)
                            {
                                CardCmd.Upgrade(randomSkill, CardPreviewStyle.None);
                            }
                            
                            CardPileAddResult addResult = await CardPileCmd.AddGeneratedCardToCombat(
                                randomSkill, 
                                PileType.Draw, 
                                player, 
                                CardPilePosition.Random
                            );
                            
                            CardCmd.PreviewCardPileAdd(addResult, 1.2f, CardPreviewStyle.HorizontalLayout);
                        }
                    }
                }
                else
                {
                    await CreatureCmd.TriggerAnim(player.Creature, "Buff", player.Character!.CastAnimDelay);

                    if (player is MegaCrit.Sts2.Core.Entities.Players.Player p)
                    {
                        await PowerCmd.Apply<IdsShikigamiPower>(choiceContext, p.Creature, 1m, p.Creature, this, false);
                    }
                }
            }
            catch (Exception e)
            {
                MegaCrit.Sts2.Core.Logging.Log.Error($"[IdsShikigami] ERROR: {e}");
            }
        }

        protected override void OnUpgrade()
        {
            if (!KomeijiKoishi.Config.KoishiBalanceManager.IsEnabled)
            {
                base.EnergyCost.UpgradeBy(-1);
            }
        }
    }
}