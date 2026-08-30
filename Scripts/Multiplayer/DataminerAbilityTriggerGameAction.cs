using System.Threading.Tasks;
using KomeijiKoishi.Dataminer;
using KomeijiKoishi.Powers;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
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

    public DataminerAbilityTriggerGameAction(
        Player player,
        int powerOrdinal,
        DataminerAbilityTriggerKind trigger,
        DataminerSubEffect effect)
    {
        _player = player;
        _powerOrdinal = powerOrdinal;
        _trigger = trigger;
        _effect = effect;
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

        return power?.ResolveQueuedEffect(_trigger, _effect) ?? Task.CompletedTask;
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
            PileScope = _effect.PileScope ?? DataminerPileScope.Hand
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

    public GameAction ToGameAction(Player player) =>
        new DataminerAbilityTriggerGameAction(
            player,
            PowerOrdinal,
            Trigger,
            new DataminerSubEffect(
                EffectKind,
                EffectAmount,
                string.IsNullOrEmpty(EffectPowerId) ? null : EffectPowerId,
                pileScope: HasPileScope ? PileScope : null));

    public void Serialize(PacketWriter writer)
    {
        writer.WriteInt(PowerOrdinal);
        writer.WriteInt((int)Trigger);
        writer.WriteInt((int)EffectKind);
        writer.WriteInt(EffectAmount);
        writer.WriteString(EffectPowerId ?? string.Empty);
        writer.WriteBool(HasPileScope);
        writer.WriteInt((int)PileScope);
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
    }
}
