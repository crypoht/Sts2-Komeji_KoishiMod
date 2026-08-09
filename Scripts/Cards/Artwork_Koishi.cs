using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using KomeijiKoishi.Enums;
using KomeijiKoishi.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;

namespace KomeijiKoishi.Cards
{
    public interface IArtworkEvolutionCard
    {
        bool ShouldEvolve { get; }

        CardModel CreateEvolution();
    }

    public abstract class ArtworkStage_Koishi : CustomCardModel, IArtworkEvolutionCard
    {
        private readonly decimal _baseValue;
        private readonly decimal _upgradeValue;

        protected ArtworkStage_Koishi(CardRarity rarity, decimal baseValue, decimal upgradeValue)
            : base(1, CardType.Attack, rarity, TargetType.AnyEnemy, true)
        {
            _baseValue = baseValue;
            _upgradeValue = upgradeValue;
        }

        public override int MaxUpgradeLevel => 2;

        public override string PortraitPath => KoishiImagePaths.CardPortrait(GetType());

        public override CardPoolModel VisualCardPool => ModelDb.CardPool<KoishiCardPool>();

        public bool ShouldEvolve => CurrentUpgradeLevel >= MaxUpgradeLevel;

        protected override IEnumerable<DynamicVar> CanonicalVars => new List<DynamicVar>
        {
            new DamageVar(_baseValue, ValueProp.Move),
            new BlockVar(_baseValue, ValueProp.Move),
            new ArtworkEvolutionHintVar()
        };

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Target != null)
            {
                await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                    .FromCard(this, cardPlay)
                    .Targeting(cardPlay.Target)
                    .WithHitFx("vfx/vfx_attack_slash", null, null)
                    .Execute(choiceContext);
            }

            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay, false);
        }

        protected override void OnUpgrade()
        {
            if (CurrentUpgradeLevel != 1)
            {
                return;
            }

            decimal increase = _upgradeValue - _baseValue;
            DynamicVars.Damage.UpgradeValueBy(increase);
            DynamicVars.Block.UpgradeValueBy(increase);
        }

        public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
        {
            if (card != this || oldPileType != PileType.Play || !ShouldEvolve || Pile == null || Pile.Type == PileType.Play || !IsTransformable)
            {
                return;
            }

            await KomeijiKoishi.Patches.ArtworkEvolutionPatch.EvolveCards(new[] { this }, CardPreviewStyle.HorizontalLayout);
        }

        public abstract CardModel CreateEvolution();

        protected T CreateEvolvedCard<T>() where T : CardModel
        {
            T replacement = CardScope!.CreateCard<T>(Owner);
            CopyEnchantmentTo(replacement);
            return replacement;
        }

        private void CopyEnchantmentTo(CardModel replacement)
        {
            EnchantmentModel? enchantment = Enchantment;
            if (enchantment == null)
            {
                return;
            }

            EnchantmentModel copied = (EnchantmentModel)enchantment.MutableClone();
            if (copied.CanEnchant(replacement))
            {
                CardCmd.Enchant(copied, replacement, copied.Amount);
            }
        }

        private sealed class ArtworkEvolutionHintVar : StringVar
        {
            private ArtworkStage_Koishi? _card;
            private static readonly LocString HintText = new("cards", "KOMEIJIKOISHI-ARTWORK_EVOLUTION_HINT");

            public ArtworkEvolutionHintVar()
                : base("ArtworkEvolutionHint")
            {
            }

            public override void SetOwner(AbstractModel owner)
            {
                base.SetOwner(owner);
                _card = owner as ArtworkStage_Koishi;
            }

            public override string ToString()
            {
                if (_card == null || (!_card.IsUpgraded && !_card.UpgradePreviewType.IsPreview()))
                {
                    return string.Empty;
                }

                return HintText.GetFormattedText();
            }
        }
    }

    [Pool(typeof(KoishiCardPool))]
    public sealed class Artwork_Koishi : ArtworkStage_Koishi
    {
        public Artwork_Koishi()
            : base(CardRarity.Common, 6m, 8m)
        {
        }

        public override CardModel CreateEvolution()
        {
            return CreateEvolvedCard<TrueArtwork_Koishi>();
        }
    }

    [Pool(typeof(EventCardPool))]
    public sealed class TrueArtwork_Koishi : ArtworkStage_Koishi
    {
        public TrueArtwork_Koishi()
            : base(CardRarity.Event, 11m, 15m)
        {
        }

        public override CardModel CreateEvolution()
        {
            return CreateEvolvedCard<TrueArtworkLiberated_Koishi>();
        }
    }

    [Pool(typeof(EventCardPool))]
    public sealed class TrueArtworkLiberated_Koishi : ArtworkStage_Koishi
    {
        public TrueArtworkLiberated_Koishi()
            : base(CardRarity.Event, 20m, 26m)
        {
        }

        public override CardModel CreateEvolution()
        {
            return CreateEvolvedCard<TrueUnconsciousArtworkLiberated_Koishi>();
        }
    }

    [Pool(typeof(TokenCardPool))]
    public sealed class TrueUnconsciousArtworkLiberated_Koishi : ArtworkStage_Koishi
    {
        public TrueUnconsciousArtworkLiberated_Koishi()
            : base(CardRarity.Ancient, 33m, 41m)
        {
        }

        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { KoishiTags.Subconscious };

        public override CardModel CreateEvolution()
        {
            return CreateEvolvedCard<Artwork_Koishi>();
        }
    }
}
