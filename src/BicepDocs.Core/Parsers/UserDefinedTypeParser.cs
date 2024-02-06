using System.Collections.Immutable;
using System.DirectoryServices.ActiveDirectory;
using Azure.Deployments.Core.Definitions.Schema;
using System.Reflection.Metadata;
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
                Properties: new List<ParsedUserDefinedTypeProperty>()
            );
            userDefinedTypes.Add(userDefinedType);

            if (typeDeclaration.Value is ObjectTypeSyntax typeObjectSynax)
            {
                userDefinedType.Properties = ParseProperties(model, typeObjectSynax);
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

            var symbol = GetUserDefinedTypeSymbol(model, typeDeclaration.Name);
            
            if (symbol != null)
            {
                userDefinedType.Description = GetStringDecorator(typeDeclaration, LanguageConstants.MetadataDescriptionPropertyName) ?? "";
            }


        }

        return userDefinedTypes.ToImmutableList();
    }

    private static List<ParsedUserDefinedTypeProperty> ParseProperties(SemanticModel model, ObjectTypeSyntax properties)
    {
        
        var parsedUserDefinedTypeProperties = new List<ParsedUserDefinedTypeProperty>();

        foreach (var property in properties.Properties)
        {
            var propertyName = property.Key.ToText();

            var userDefinedTypeProperty = new ParsedUserDefinedTypeProperty(propertyName,"");

            if (property.Value is UnionTypeSyntax unionType) // Contains a list with allowed values
            {
                userDefinedTypeProperty.Type = unionType.ToText();

                var allowList = unionType.Members.Select(x => x.Value.ToText()).ToArray();
                var allowValues = allowList.Select(x => x.Replace("'", "")).Where(x => !string.IsNullOrEmpty(x)).ToList();
                userDefinedTypeProperty.IsComplexAllow = allowList.Length > 2;
                userDefinedTypeProperty.AllowedValues = allowValues;
            }
            else if (property.Value is VariableAccessSyntax variableAccessSyntax)
            {
                userDefinedTypeProperty.Type = variableAccessSyntax.Name.IdentifierName;
            }
            else if (property.Value is NullableTypeSyntax nullableTypeSyntax)
            {
                userDefinedTypeProperty.Type = ((VariableAccessSyntax)nullableTypeSyntax.Base).Name.IdentifierName;
                userDefinedTypeProperty.IsRequired = false;
            }

            userDefinedTypeProperty.Description = GetStringDecorator(property, LanguageConstants.MetadataDescriptionPropertyName);
            userDefinedTypeProperty.MaxLength = GetDecorator(property, LanguageConstants.ParameterMaxLengthPropertyName);
            userDefinedTypeProperty.MinLength = GetDecorator(property, LanguageConstants.ParameterMinLengthPropertyName);
            userDefinedTypeProperty.Secure = HasDecorator(property, LanguageConstants.ParameterSecurePropertyName);
            userDefinedTypeProperty.MinValue = GetDecorator(property, LanguageConstants.ParameterMinValuePropertyName);
            userDefinedTypeProperty.MaxValue = GetDecorator(property, LanguageConstants.ParameterMaxValuePropertyName);

            parsedUserDefinedTypeProperties.Add(userDefinedTypeProperty);
        }

        return parsedUserDefinedTypeProperties;

    }

    private static bool HasDecorator(ObjectTypePropertySyntax objectTypeProperty, string decoratorName)
    {
        var f = objectTypeProperty.Decorators;
        var fcs = f.FirstOrDefault(x => (x.Expression as FunctionCallSyntax)?.Name?.IdentifierName == decoratorName);
        return fcs != null;
    }

    private static TypeAliasSymbol? GetUserDefinedTypeSymbol(SemanticModel model, string userDefinedTypeName) =>
        model.Root.TypeDeclarations.FirstOrDefault(x => x.Name == userDefinedTypeName);


    private static TypeAliasSymbol? GetUserDefinedTypePropertySymbol(SemanticModel model, string userDefinedTypeName) =>
        model.Root.TypeDeclarations.FirstOrDefault(x => x.Name == userDefinedTypeName);


    private static int? GetDecorator(ObjectTypePropertySyntax objectTypeProperty, string decoratorName)
    {
        var f = objectTypeProperty.Decorators;
        var fcs = f.FirstOrDefault(x => (x.Expression as FunctionCallSyntax)?.Name?.IdentifierName == decoratorName);
        var literalText = (fcs?.Arguments.FirstOrDefault()?.Expression as IntegerLiteralSyntax)?.Literal?.Text;
        return int.TryParse(literalText, out var value) ? value : default(int?);
    }

    private static string? GetStringDecorator(ObjectTypePropertySyntax objectTypeProperty, string decoratorName)
    {
        var f = objectTypeProperty.Decorators;
        var fcs = f.FirstOrDefault(x => (x.Expression as FunctionCallSyntax)?.Name?.IdentifierName == decoratorName);
        var literalText = (fcs?.Arguments.FirstOrDefault()?.Expression as StringSyntax)?.SegmentValues.LastOrDefault();
        return literalText;
    }

    private static string? GetStringDecorator(TypeAliasSymbol typeSymbol, string decoratorName)
    {
        var f = typeSymbol.DeclaringType.Decorators;
        var fcs = f.FirstOrDefault(x => (x.Expression as FunctionCallSyntax)?.Name?.IdentifierName == decoratorName);
        var literal = (fcs?.Arguments.FirstOrDefault()?.Expression as StringSyntax)?.SegmentValues.LastOrDefault();
        return literal;
    }

}