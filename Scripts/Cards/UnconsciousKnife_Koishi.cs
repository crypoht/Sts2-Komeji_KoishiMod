using BaseLib.Abstracts;
using BaseLib.Utils;
using HarmonyLib;
using KomeijiKoishi.Enums;
using KomeijiKoishi.Pools;
using KomeijiKoishi.Utils_Koishi;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace KomeijiKoishi.Cards;

[Pool(typeof(KoishiCardPool))]
public sealed class UnconsciousKnife_Koishi : CustomCardModel
{
    private static readonly PileType[] Destinations = [PileType.Draw, PileType.Hand, PileType.Discard, PileType.Exhaust];

    public UnconsciousKnife_Koishi() : base(514, CardType.Attack, CardRarity.Ancient, TargetType.None, true) { }

    public override string PortraitPath => KoishiImagePaths.CardNormalPortrait(GetType());
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Shiv, KoishiTags.Subconscious];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5m, ValueProp.Move), new RepeatVar(14)];

    public override async Task AfterCardDrawnEarly(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card == this && card.Pile?.Type == PileType.Hand && !CombatManager.Instance.IsOverOrEnding)
            await KoishiExtensions.SafeAutoPlayCard(choiceContext, Owner, this);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState is not { } combat)
            return;

        if (Owner.Creature.HasPower<FanOfKnivesPower>())
        {
            var attack = DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .WithHitCount(DynamicVars.Repeat.IntValue)
                .TargetingAllOpponents(combat)
                .WithHitFx("vfx/vfx_attack_slash", null, "knife_attack.mp3");
            if (!IsUpgraded)
                attack.SpawningHitVfxOnEachCreature();

            await attack.Execute(choiceContext);
            return;
        }

        for (int hit = 0; hit < DynamicVars.Repeat.IntValue && Owner.Creature.IsAlive && !CombatManager.Instance.IsOverOrEnding; hit++)
        {
            var targets = IsUpgraded
                ? combat.HittableEnemies
                : combat.Creatures.Where(creature => creature.IsHittable).ToList();
            Creature? target = Owner.RunState.Rng.CombatTargets.NextItem(targets);
            if (target == null)
                break;

            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .Targeting(target)
                .WithHitFx("vfx/vfx_attack_slash", null, "knife_attack.mp3")
                .Execute(choiceContext);
        }
    }

    [HarmonyPatch(typeof(AttackCommand), "GetPossibleTargets")]
    private static class FanOfKnivesTargetsPatch
    {
        private static void Postfix(AttackCommand __instance, ref IReadOnlyList<Creature> __result)
        {
            // Only the unupgraded knife's Fan of Knives branch uses a multi-target attack.
            if (__instance.ModelSource is not UnconsciousKnife_Koishi { IsUpgraded: false }
                || !__instance.IsMultiTargeted || __instance.IsRandomlyTargeted
                || __instance.Attacker?.CombatState is not { } combat)
                return;

            __result = CombatManager.Instance.IsOverOrEnding
                ? Array.Empty<Creature>()
                : combat.Creatures.Where(creature => creature.IsHittable).ToList();
        }
    }

#if STS2_BETA
    protected override CardLocation GetResultLocationForCardPlay()
    {
        CardLocation location = base.GetResultLocationForCardPlay();
        if (location.pileType == PileType.Discard && CombatState != null)
            location.pileType = Owner.RunState.Rng.CombatCardSelection.NextItem(Destinations);
        return location;
    }
#else
    protected override PileType GetResultPileTypeForCardPlay()
    {
        PileType pile = base.GetResultPileTypeForCardPlay();
        return pile == PileType.Discard && CombatState != null
            ? Owner.RunState.Rng.CombatCardSelection.NextItem(Destinations)
            : pile;
    }
#endif
}
