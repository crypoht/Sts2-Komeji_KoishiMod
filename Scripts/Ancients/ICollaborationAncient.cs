namespace KomeijiKoishi.Ancients;

/// <summary>Declares the loaded mod IDs required for this Ancient's relic compendium group.</summary>
public interface ICollaborationAncient
{
    IReadOnlyList<string> RequiredModIds { get; }
}
