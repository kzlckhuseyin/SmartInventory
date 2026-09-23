using Microsoft.AspNetCore.Mvc.TagHelpers;

public class Manufacturer : BaseEntitiy
{
    public string Name { get; set; } = string.Empty;
    public string? Country { get; set; }
    public bool IsRestricted { get; set; } // Embargo/restriction status

    // Navigation Property
    public ICollection<Part> Parts { get; set; } = new List<Part>();
}