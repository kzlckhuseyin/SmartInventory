public class Datasheet : BaseEntitiy
{
    public Guid? PartId { get; set; } // The PartId might not have been generated yet upon initial load.
    public string FilePath { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public ProcessingStatus Status { get; set; } = ProcessingStatus.Pending;
    public string? ExtractedRawText { get; set; } // Trimmed raw text extracted from PDF

    // Navigation Property
    public Part? Part { get; set; }
}