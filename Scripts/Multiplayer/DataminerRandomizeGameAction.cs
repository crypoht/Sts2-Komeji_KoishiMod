using System.Threading.Tasks;
using KomeijiKoishi.Patches;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace KomeijiKoishi.Multiplayer;

/// <summary>
/// Synchronizes a Dataminer right-click before any random result is generated.
/// </summary>
public sealed class DataminerRandomizeGameAction : GameAction
{
    private readonly Player _player;
    private readonly int _relicOrdinal;
    private readonly string _serializedPlan;

    public DataminerRandomizeGameAction(Player player, int relicOrdinal)
    {
        _player = player;
        _relicOrdinal = relicOrdinal;
        _serializedPlan = DataminerRelicPatch.CreateRandomizationPlan(player, relicOrdinal);
    }

    public DataminerRandomizeGameAction(Player player, int relicOrdinal, string serializedPlan)
    {
        _player = player;
        _relicOrdinal = relicOrdinal;
        _serializedPlan = serializedPlan;
    }

    public override ulong OwnerId => _player.NetId;

    public override GameActionType ActionType => GameActionType.CombatPlayPhaseOnly;

    protected override Task ExecuteAction()
    {
        return DataminerRelicPatch.RandomizeHandForAction(
            _player,
            _relicOrdinal,
            _serializedPlan);
    }

    public override INetAction ToNetAction()
    {
        return new NetDataminerRandomizeAction
        {
            RelicOrdinal = _relicOrdinal,
            SerializedPlan = _serializedPlan
        };
    }
}

public struct NetDataminerRandomizeAction : INetAction, IPacketSerializable
{
    public int RelicOrdinal;
    public string SerializedPlan;

    public GameAction ToGameAction(Player player)
    {
        return new DataminerRandomizeGameAction(player, RelicOrdinal, SerializedPlan);
    }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteInt(RelicOrdinal);
        writer.WriteString(SerializedPlan ?? string.Empty);
    }

    public void Deserialize(PacketReader reader)
    {
        RelicOrdinal = reader.ReadInt();
        SerializedPlan = reader.ReadString();
    }
}
