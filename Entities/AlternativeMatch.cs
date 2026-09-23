public class AlternativeMatch : BaseEntitiy
{
    public Guid SourcePartId { get; set; } // Requested/Original part
    public Guid TargetPartId { get; set; } // Recommended replacement part
    public int MatchScore { get; set; } // Compatibility score between 0 and 100
    public string MatchReason { get; set; } = string.Empty; // E.g., "The pinout and package are identical; the voltage range is wider."
    public bool VerifiedByHuman { get; set; } = false; // Did the engineer approve it manually?

    // Navigation Properties
    public Part SourcePart { get; set; } = null!;
    public Part TargetPart { get; set; } = null!;
}