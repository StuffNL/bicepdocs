using Bicep.Core.Syntax;

namespace LandingZones.Tools.BicepDocs.Core.Models.Parsing;


public record ParsedUserDefinedType(string Name, List<ParsedUserDefinedTypeProperty> Properties)
{
    public string Name { get; set; } = Name;

    public string Description { get; set; }

    public List<ParsedUserDefinedTypeProperty> Properties { get; set; } = Properties;

}