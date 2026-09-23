public class Part : BaseEntitiy
{
    public Guid ManufacturerId { get; set; }
    public string PartNumber { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string PackageType { get; set; } = string.Empty; // E.g. LQFP-100, SOIC-8
    public int PinCount { get; set; }

    public decimal MinOperatingTemp { get; set; } // °C
    public decimal MaxOperatingTemp { get; set; } // °C
    public decimal MinVoltage { get; set; } // V
    public decimal MaxVoltage { get; set; } // V

    public bool IsMilSpec { get; set; } = false; // Military standard?
    public bool IsApproved { get; set; } = false; // Did it pass the engineer's approval?

    // PostgreSQL JSONB field - Category-specific extra parameters
    public string? AdditionalFeatures { get; set; }

    // Navigation Properties
    public Manufacturer Manufacturer { get; set; } = null!;
    public ICollection<Datasheet> Datasheets { get; set; } = new List<Datasheet>();

    // Self-Referencing
    public ICollection<AlternativeMatch> SourceMatches { get; set; } = new List<AlternativeMatch>();
    public ICollection<AlternativeMatch> TargetMatches { get; set; } = new List<AlternativeMatch>();
}