using Bicep.Core.Syntax;

namespace LandingZones.Tools.BicepDocs.Core.Models.Parsing;


public record ParsedUserDefinedTypeProperty(string Name)
{
    public string Name { get; set; } = Name;
    public string Type { get; set; }
    public string? Description { get; set; }
    public bool IsComplexDefault { get; set; }
    public List<string>? AllowedValues { get; set; }
    public bool IsComplexAllow { get; set; }
    public int? MinLength { get; set; }

    public int? MinValue { get; set; }

    public int? MaxLength { get; set; }

    public int? MaxValue { get; set; }

    public bool Secure { get; set; }
}