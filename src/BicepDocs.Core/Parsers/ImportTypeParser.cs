using Bicep.Core.Semantics;
using Bicep.Core.TypeSystem.Types;
using LandingZones.Tools.BicepDocs.Core.Models.Parsing;
using System.Collections.Immutable;

namespace LandingZones.Tools.BicepDocs.Core.Parsers;

public static class ImportTypeParser
{
    public static ImmutableList<ParsedUserDefinedType> ParseImportTypes(SemanticModel model)
    {
        var userDefinedTypes = new List<ParsedUserDefinedType>();

        foreach (var importedType in model.Root.ImportedTypes.OrderBy(x => x.Name))
        {
            var userDefinedType = new ParsedUserDefinedType
            (
                Name: importedType.Name,
                Properties: new List<ParsedUserDefinedTypeProperty>()
            )
            {
                Description = importedType.Description,
                IsPrimitiveLiteral = false
            };

            userDefinedTypes.Add(userDefinedType);

            var objectType = ((importedType.ExportMetadata.TypeReference as TypeType).Unwrapped as ObjectType);
            var propertyTypes = PropertyParser.GetPropertyTypes(objectType);

            var props = ((importedType.ExportMetadata.TypeReference as TypeType).Unwrapped as ObjectType).Properties;
            userDefinedType.Properties = PropertyParser.ParseProperties(props, propertyTypes);

        }
        return userDefinedTypes.ToImmutableList();
    }

    
}
