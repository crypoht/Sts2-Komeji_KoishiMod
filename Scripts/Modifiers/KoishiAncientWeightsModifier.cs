using KomeijiKoishi.Config;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using System.Text.Json;

namespace KomeijiKoishi.Modifiers
{
    public sealed class KoishiAncientWeightsModifier : ModifierModel
    {
        public override bool ShouldReceiveCombatHooks => false;

        protected override string IconPath => "res://mods/Komeiji_Koishi/images/ancients/HakureiReimu_Koishi_map_icon.png";

        public override LocString Description
        {
            get
            {
                LocString description = base.Description;
                description.Add("NextAncientProbabilities", BuildNextAncientProbabilitiesText());
                return description;
            }
        }

        [SavedProperty] public int MoriyaTwoGodsWeight { get; private set; } = 1;
        [SavedProperty] public int HakureiReimuWeight { get; private set; } = 1;
        [SavedProperty] public int OrobasWeight { get; private set; } = 1;
        [SavedProperty] public int PaelWeight { get; private set; } = 1;
        [SavedProperty] public int TezcataraWeight { get; private set; } = 1;
        [SavedProperty] public int NonupeipeWeight { get; private set; } = 1;
        [SavedProperty] public int TanxWeight { get; private set; } = 1;
        [SavedProperty] public int VakuuWeight { get; private set; } = 1;
        [SavedProperty] public int DarvWeight { get; private set; } = 1;
        [SavedProperty] public string ExternalAncientWeights { get; private set; } = "";

        public IReadOnlyDictionary<string, int> ExternalWeights => ParseExternalWeights(ExternalAncientWeights);

        public void Apply(AncientWeights weights, IReadOnlyDictionary<string, int>? externalWeights = null)
        {
            MoriyaTwoGodsWeight = ClampWeight(weights.MoriyaTwoGods);
            HakureiReimuWeight = ClampWeight(weights.HakureiReimu);
            OrobasWeight = ClampWeight(weights.Orobas);
            PaelWeight = ClampWeight(weights.Pael);
            TezcataraWeight = ClampWeight(weights.Tezcatara);
            NonupeipeWeight = ClampWeight(weights.Nonupeipe);
            TanxWeight = ClampWeight(weights.Tanx);
            VakuuWeight = ClampWeight(weights.Vakuu);
            DarvWeight = ClampWeight(weights.Darv);
            ExternalAncientWeights = externalWeights == null
                ? ""
                : JsonSerializer.Serialize(externalWeights.ToDictionary(pair => pair.Key, pair => ClampWeight(pair.Value)));
        }

        public AncientWeights ToAncientWeights()
        {
            return new AncientWeights(
                ClampWeight(MoriyaTwoGodsWeight),
                ClampWeight(HakureiReimuWeight),
                ClampWeight(OrobasWeight),
                ClampWeight(PaelWeight),
                ClampWeight(TezcataraWeight),
                ClampWeight(NonupeipeWeight),
                ClampWeight(TanxWeight),
                ClampWeight(VakuuWeight),
                ClampWeight(DarvWeight));
        }

        private static int ClampWeight(int weight)
        {
            return int.Clamp(weight, 0, 10);
        }

        private static IReadOnlyDictionary<string, int> ParseExternalWeights(string serializedWeights)
        {
            if (string.IsNullOrWhiteSpace(serializedWeights))
            {
                return new Dictionary<string, int>();
            }

            try
            {
                Dictionary<string, int>? weights = JsonSerializer.Deserialize<Dictionary<string, int>>(serializedWeights);
                return weights?.ToDictionary(pair => pair.Key, pair => ClampWeight(pair.Value)) ?? new Dictionary<string, int>();
            }
            catch
            {
                return new Dictionary<string, int>();
            }
        }

        private string BuildNextAncientProbabilitiesText()
        {
            int currentActIndex = Math.Max(RunState?.CurrentActIndex ?? 0, KoishiModConfig.ActiveRunCurrentActIndex);
            int currentActNumber = currentActIndex + 1;
            if (currentActNumber > 2)
            {
                return AncientProbabilityChart.LocalizedNone();
            }

            IReadOnlyList<AncientProbabilityEntry> entries =
                AncientProbabilityData.GetActEntries(currentActNumber + 1, ToAncientWeights());
            if (entries.Count == 0)
            {
                return AncientProbabilityChart.LocalizedNone();
            }

            return string.Join(
                "\n",
                entries.Select(entry => $"{AncientProbabilityChart.LocalizedAncientName(entry.Info)}: {entry.Probability * 100f:0.#}%"));
        }
    }
}
