using System.Linq;
using System.Threading.Tasks;
using KomeijiKoishi.Dataminer;
using KomeijiKoishi.Powers;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace KomeijiKoishi.Multiplayer;

public sealed class DataminerAbilityTriggerGameAction : GameAction
{
    private readonly Player _player;
    private readonly int _powerOrdinal;
    private readonly DataminerAbilityTriggerKind _trigger;
    private readonly DataminerSubEffect _effect;
    private readonly int _targetCombatId;

    public DataminerAbilityTriggerGameAction(
        Player player,
        int powerOrdinal,
        DataminerAbilityTriggerKind trigger,
        DataminerSubEffect effect,
        Creature? target = null)
        : this(player, powerOrdinal, trigger, effect,
            target == null ? -1 : checked((int)target.CombatId))
    {
    }

    public DataminerAbilityTriggerGameAction(
        Player player,
        int powerOrdinal,
        DataminerAbilityTriggerKind trigger,
        DataminerSubEffect effect,
        int targetCombatId)
    {
        _player = player;
        _powerOrdinal = powerOrdinal;
        _trigger = trigger;
        _effect = effect;
        _targetCombatId = targetCombatId;
    }

    public override ulong OwnerId => _player.NetId;

    public override GameActionType ActionType => GameActionType.CombatPlayPhaseOnly;

    protected override Task ExecuteAction()
    {
        MegaCrit.Sts2.Core.Logging.Log.Info(
            $"[KoishiDataminerAbility] ExecuteAction player={_player.NetId}, powerOrdinal={_powerOrdinal}.");

        DataminerAbilityPower? power = _player.Creature.Powers
            .OfType<DataminerAbilityPower>()
            .Skip(_powerOrdinal)
            .FirstOrDefault();

        if (power == null)
        {
            MegaCrit.Sts2.Core.Logging.Log.Warn(
                $"[KoishiDataminerAbility] ExecuteAction could not find power ordinal={_powerOrdinal}.");
        }

        Creature? target = null;
        if (_targetCombatId >= 0 && _player.Creature.CombatState is { } combatState)
        {
            target = combatState.PlayerCreatures
                .Concat(combatState.Enemies)
                .FirstOrDefault(creature => creature.CombatId == _targetCombatId);
        }

        return power?.ResolveQueuedEffect(_trigger, _effect, target) ?? Task.CompletedTask;
    }

    public override INetAction ToNetAction() =>
        new NetDataminerAbilityTriggerAction
        {
            PowerOrdinal = _powerOrdinal,
            Trigger = _trigger,
            EffectKind = _effect.Kind,
            EffectAmount = _effect.Amount,
            EffectPowerId = _effect.PowerId ?? string.Empty,
            HasPileScope = _effect.PileScope.HasValue,
            PileScope = _effect.PileScope ?? DataminerPileScope.Hand,
            HasGeneratedCardKind = _effect.GeneratedCardKind.HasValue,
            GeneratedCardKind = _effect.GeneratedCardKind ?? DataminerGeneratedCardKind.Random,
            GeneratedCardId = _effect.GeneratedCardId ?? string.Empty,
            HasGeneratedCardDestination = _effect.GeneratedCardDestination.HasValue,
            GeneratedCardDestination = _effect.GeneratedCardDestination ?? DataminerGeneratedCardDestination.Hand,
            SecondaryAmount = _effect.SecondaryAmount,
            TargetCombatId = _targetCombatId
        };
}

public struct NetDataminerAbilityTriggerAction : INetAction, IPacketSerializable
{
    public int PowerOrdinal;
    public DataminerAbilityTriggerKind Trigger;
    public DataminerEffectKind EffectKind;
    public int EffectAmount;
    public string EffectPowerId;
    public bool HasPileScope;
    public DataminerPileScope PileScope;
    public bool HasGeneratedCardKind;
    public DataminerGeneratedCardKind GeneratedCardKind;
    public string GeneratedCardId;
    public bool HasGeneratedCardDestination;
    public DataminerGeneratedCardDestination GeneratedCardDestination;
    public int SecondaryAmount;
    public int TargetCombatId;

    public GameAction ToGameAction(Player player) =>
        new DataminerAbilityTriggerGameAction(
            player,
            PowerOrdinal,
            Trigger,
            new DataminerSubEffect(
                EffectKind,
                EffectAmount,
                string.IsNullOrEmpty(EffectPowerId) ? null : EffectPowerId,
                HasGeneratedCardKind ? GeneratedCardKind : null,
                string.IsNullOrEmpty(GeneratedCardId) ? null : GeneratedCardId,
                HasGeneratedCardDestination ? GeneratedCardDestination : null,
                HasPileScope ? PileScope : null,
                SecondaryAmount,
                DataminerPowerTarget.Self),
            TargetCombatId);

    public void Serialize(PacketWriter writer)
    {
        writer.WriteInt(PowerOrdinal);
        writer.WriteInt((int)Trigger);
        writer.WriteInt((int)EffectKind);
        writer.WriteInt(EffectAmount);
        writer.WriteString(EffectPowerId ?? string.Empty);
        writer.WriteBool(HasPileScope);
        writer.WriteInt((int)PileScope);
        writer.WriteBool(HasGeneratedCardKind);
        writer.WriteInt((int)GeneratedCardKind);
        writer.WriteString(GeneratedCardId ?? string.Empty);
        writer.WriteBool(HasGeneratedCardDestination);
        writer.WriteInt((int)GeneratedCardDestination);
        writer.WriteInt(SecondaryAmount);
        writer.WriteInt(TargetCombatId);
    }

    public void Deserialize(PacketReader reader)
    {
        PowerOrdinal = reader.ReadInt();
        Trigger = (DataminerAbilityTriggerKind)reader.ReadInt();
        EffectKind = (DataminerEffectKind)reader.ReadInt();
        EffectAmount = reader.ReadInt();
        EffectPowerId = reader.ReadString();
        HasPileScope = reader.ReadBool();
        PileScope = (DataminerPileScope)reader.ReadInt();
        HasGeneratedCardKind = reader.ReadBool();
        GeneratedCardKind = (DataminerGeneratedCardKind)reader.ReadInt();
        GeneratedCardId = reader.ReadString();
        HasGeneratedCardDestination = reader.ReadBool();
        GeneratedCardDestination = (DataminerGeneratedCardDestination)reader.ReadInt();
        SecondaryAmount = reader.ReadInt();
        TargetCombatId = reader.ReadInt();
    }
}
