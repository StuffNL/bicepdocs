using System.Collections.Immutable;
using Bicep.Core;
using Bicep.Core.Navigation;
using Bicep.Core.Semantics;
using Bicep.Core.Syntax;
using Bicep.Core.TypeSystem;
using LandingZones.Tools.BicepDocs.Core.Models.Parsing;

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
                Properties: ParseProperties(model, typeDeclaration.Type.Name)
            );

            userDefinedTypes.Add(userDefinedType);
        }

        return userDefinedTypes.ToImmutableList();
    }

    private static List<ParsedUserDefinedTypeProperty> ParseProperties(SemanticModel model, string properties)
    {
        //TODO: replace with REGEX
        properties = properties.Replace(@"Type<{", "").Replace(@"Type<", "").Replace(@"}>", "").Replace("'","").Replace(">","").Trim();
        var parsedUserDefinedTypeProperties = new List<ParsedUserDefinedTypeProperty>();

        foreach (var property in properties.Split(','))
        {
            var propertyName = property.Split(':').First();
            var propertyType = property.Split(':').Last();
            var userDefinedTypeProperty = new ParsedUserDefinedTypeProperty(propertyName, propertyType);

            var allowList = propertyType.Split('|').Select(x => x.Trim()).Where(x => x.Length > 1).ToArray();
            var allowValues = allowList.Select(x => x.Replace("'", "")).Where(x => !string.IsNullOrEmpty(x)).ToList();
            userDefinedTypeProperty.IsComplexAllow = allowList.Length > 2;
            userDefinedTypeProperty.AllowedValues = allowValues;

            var symbol = GetUserDefinedTypeSymbol(model, propertyType);
            if (symbol == null)
            {
                parsedUserDefinedTypeProperties.Add(userDefinedTypeProperty);
                continue;
            }

            userDefinedTypeProperty.Description =
                GetStringDecorator(symbol, LanguageConstants.MetadataDescriptionPropertyName);
            userDefinedTypeProperty.MaxLength = GetDecorator(symbol, LanguageConstants.ParameterMaxLengthPropertyName);
            userDefinedTypeProperty.MinLength = GetDecorator(symbol, LanguageConstants.ParameterMinLengthPropertyName);
            userDefinedTypeProperty.Secure = HasDecorator(symbol, LanguageConstants.ParameterSecurePropertyName);
            userDefinedTypeProperty.MinValue = GetDecorator(symbol, LanguageConstants.ParameterMinValuePropertyName);
            userDefinedTypeProperty.MaxValue = GetDecorator(symbol, LanguageConstants.ParameterMaxValuePropertyName);
        }

        return parsedUserDefinedTypeProperties;

    }


    private static bool HasDecorator(TypeAliasSymbol typeSymbol, string decoratorName)
    {
        var f = typeSymbol.DeclaringType.Decorators;
        var fcs = f.FirstOrDefault(x => (x.Expression as FunctionCallSyntax)?.Name?.IdentifierName == decoratorName);
        return fcs != null;
    }

    private static TypeAliasSymbol? GetUserDefinedTypeSymbol(SemanticModel model, string userDefinedTypeName) =>
        model.Root.TypeDeclarations.FirstOrDefault(x => x.Name == userDefinedTypeName);


    private static int? GetDecorator(TypeAliasSymbol typeSymbol, string decoratorName)
    {
        var f = typeSymbol.DeclaringType.Decorators;
        var fcs = f.FirstOrDefault(x => (x.Expression as FunctionCallSyntax)?.Name?.IdentifierName == decoratorName);
        var literalText = (fcs?.Arguments.FirstOrDefault()?.Expression as IntegerLiteralSyntax)?.Literal?.Text;
        return int.TryParse(literalText, out var value) ? value : default(int?);
    }

    private static string? GetStringDecorator(TypeAliasSymbol typeSymbol, string decoratorName)
    {
        var f = typeSymbol.DeclaringType.Decorators;
        var fcs = f.FirstOrDefault(x => (x.Expression as FunctionCallSyntax)?.Name?.IdentifierName == decoratorName);
        var literalText = (fcs?.Arguments.FirstOrDefault()?.Expression as IntegerLiteralSyntax)?.Literal?.Text;
        return literalText;
    }

}