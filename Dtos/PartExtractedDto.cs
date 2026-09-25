public class PartExtractedDto
{
    public string PartNumber { get; set; } = string.Empty;
    public string ManufacturerName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string PackageType { get; set; } = string.Empty;
    public int PinCount { get; set; }
    public decimal MinOperatingTemp { get; set; }
    public decimal MaxOperatingTemp { get; set; }
    public decimal MinVoltage { get; set; }
    public decimal MaxVoltage { get; set; }
    public bool IsMilSpec { get; set; }
    public Dictionary<string, object> AdditionalFeatures { get; set; } = new();
}