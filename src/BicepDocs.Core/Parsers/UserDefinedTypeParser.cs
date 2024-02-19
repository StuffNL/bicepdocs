using Bicep.Core;
using Bicep.Core.Navigation;
using Bicep.Core.Semantics;
using Bicep.Core.Syntax;
using Bicep.Core.TypeSystem.Types;
using LandingZones.Tools.BicepDocs.Core.Models.Parsing;
using System.Collections.Immutable;

namespace LandingZones.Tools.BicepDocs.Core.Parsers;

public static class UserDefinedTypeParser
{
    public static ImmutableList<ParsedUserDefinedType> ParseUserDefinedTypes(SemanticModel model)
    {
        var userDefinedTypes = new List<ParsedUserDefinedType>();

        foreach (var typeDeclaration in model.Root.TypeDeclarations.OrderBy(x => x.Name))
        {

            var userDefinedType = new ParsedUserDefinedType
            (
                Name: typeDeclaration.Name,
                Properties: new List<ParsedUserDefinedTypeProperty>()
            );

            userDefinedType.Description =
                GetStringDecorator(typeDeclaration, LanguageConstants.MetadataDescriptionPropertyName) ?? "";


            userDefinedTypes.Add(userDefinedType);
            if ((typeDeclaration.Type as TypeType).Unwrapped is ObjectType objectType)
            {
                var propertyTypes = PropertyParser.GetPropertyTypes(objectType);
                userDefinedType.Properties = PropertyParser.ParseProperties(objectType.Properties, propertyTypes);
            }
            if (typeDeclaration.Value is UnionTypeSyntax unionTypeSyntax) // IsPrimitiveLiteral Union type
            {
                userDefinedType.Description = typeDeclaration.Value.ToText();
                userDefinedType.IsPrimitiveLiteral = true;
                continue;
            }
            if (typeDeclaration.Value is IntegerLiteralSyntax integerLiteralSyntax) // IsPrimitiveLiteral Int type
            {
                userDefinedType.Description = integerLiteralSyntax.ToText();
                userDefinedType.IsPrimitiveLiteral = true;
                continue;
            }
            if (typeDeclaration.Value is StringSyntax stringSyntax) // IsPrimitiveLiteral Int type
            {
                userDefinedType.Description = stringSyntax.ToText();
                userDefinedType.IsPrimitiveLiteral = true;
                continue;
            }
        }

        return userDefinedTypes.ToImmutableList();
    }
    
    private static string? GetStringDecorator(TypeAliasSymbol typeSymbol, string decoratorName)
    {
        var f = typeSymbol.DeclaringType.Decorators;
        var fcs = f.FirstOrDefault(x => (x.Expression as FunctionCallSyntax)?.Name?.IdentifierName == decoratorName);
        var literal = (fcs?.Arguments.FirstOrDefault()?.Expression as StringSyntax)?.SegmentValues.LastOrDefault();
        return literal;
    }

}