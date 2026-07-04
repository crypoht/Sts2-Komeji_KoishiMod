using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace KomeijiKoishi.Multiplayer;

public struct KoishiRunConfigMessage : INetMessage
{
    public bool ShouldBroadcast => true;
    public NetTransferMode Mode => NetTransferMode.Reliable;
    public LogLevel LogLevel => LogLevel.VeryDebug;
    public bool ShouldBuffer => true;

    public bool AncientsEnabled;
    public bool PlayMoriyaDanceForAllPlayers;
    public bool AncientWeightsEnabled;
    public AncientWeights Weights;
    public string ExternalWeights;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteBool(AncientsEnabled);
        writer.WriteBool(PlayMoriyaDanceForAllPlayers);
        writer.WriteBool(AncientWeightsEnabled);
        WriteWeight(writer, Weights.MoriyaTwoGods);
        WriteWeight(writer, Weights.HakureiReimu);
        WriteWeight(writer, Weights.Orobas);
        WriteWeight(writer, Weights.Pael);
        WriteWeight(writer, Weights.Tezcatara);
        WriteWeight(writer, Weights.Nonupeipe);
        WriteWeight(writer, Weights.Tanx);
        WriteWeight(writer, Weights.Vakuu);
        WriteWeight(writer, Weights.Darv);
        writer.WriteString(ExternalWeights ?? string.Empty);
    }

    public void Deserialize(PacketReader reader)
    {
        AncientsEnabled = reader.ReadBool();
        PlayMoriyaDanceForAllPlayers = reader.ReadBool();
        AncientWeightsEnabled = reader.ReadBool();
        Weights = new AncientWeights(
            ReadWeight(reader),
            ReadWeight(reader),
            ReadWeight(reader),
            ReadWeight(reader),
            ReadWeight(reader),
            ReadWeight(reader),
            ReadWeight(reader),
            ReadWeight(reader),
            ReadWeight(reader));
        ExternalWeights = reader.ReadString();
    }

    private static void WriteWeight(PacketWriter writer, int value)
    {
        writer.WriteByte((byte)System.Math.Clamp(value, 0, 10), 4);
    }

    private static int ReadWeight(PacketReader reader)
    {
        return reader.ReadByte(4);
    }
}
