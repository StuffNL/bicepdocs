using Bicep.Core.Semantics;
using Bicep.Core.TypeSystem.Types;
using LandingZones.Tools.BicepDocs.Core.Models.Parsing;
using System.Collections.Immutable;
using System.DirectoryServices.ActiveDirectory;

namespace LandingZones.Tools.BicepDocs.Core.Parsers;


public static class WildCardImportTypeParser
{
    public static ImmutableList<ParsedUserDefinedType> ParseWildcardTypes(SemanticModel model)
    {
        var userDefinedTypes = new List<ParsedUserDefinedType>();
        
        foreach (var wildcardImport in model.Root.WildcardImports.OrderBy(x => x.Name))
        {
            var userDefinedType = new ParsedUserDefinedType
            (
                Name: wildcardImport.Name,
                Properties: new List<ParsedUserDefinedTypeProperty>()
            )
            {
                Description = wildcardImport.TryGetDescriptionFromDecorator(),
                IsPrimitiveLiteral = false
            };

            userDefinedTypes.Add(userDefinedType);

            var namespaceType = (wildcardImport.Type as NamespaceType);

            var propertyTypes = namespaceType.Properties.Select(x => new
            {
                Name = x.Key,
                Type = x.Value.Name
            }).ToDictionary(x => x.Name, x => x.Type);

            userDefinedType.Properties = PropertyParser.ParseProperties(namespaceType.Properties, propertyTypes);

        }
        return userDefinedTypes.ToImmutableList();
    }

    
}
