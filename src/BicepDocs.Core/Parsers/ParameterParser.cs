using System.Collections.Immutable;
using Bicep.Core;
using Bicep.Core.Navigation;
using Bicep.Core.Semantics;
using Bicep.Core.Syntax;
using LandingZones.Tools.BicepDocs.Core.Models.Parsing;

namespace LandingZones.Tools.BicepDocs.Core.Parsers;

public static class ParameterParser
{
    public static ImmutableList<ParsedParameter> ParseParameters(SemanticModel model)
    {
        var parameters = new List<ParsedParameter>();
        foreach (var templateParameter in model.Parameters.OrderBy(x => x.Key))
        {
            var metaData = templateParameter.Value;
            var parameter = new ParsedParameter
            (
                Name: templateParameter.Key,
                Type: templateParameter.Value.TypeReference.Type.Name
            )
            {
                Description = templateParameter.Value.Description
            };

            var paramType = templateParameter.Value.TypeReference.Type.Name;
            var allowList = paramType.Split('\'').Select(x => x.Trim()).Where(x => x.Length > 1).ToArray();
            var allowValues = allowList.Select(x => x.Replace("'", "")).Where(x => !string.IsNullOrEmpty(x)).ToList();

            var symbol = GetParameterSymbol(model, templateParameter.Key);

            var typeName = templateParameter.Value.TypeReference.Type.Name;

            parameter.IsComplexAllow = allowList.Length > 2;
            parameter.AllowedValues = allowValues;
            parameter.Type = typeName;
            parameter.IsRequired = metaData.IsRequired;

            if (symbol == null)
            {
                parameters.Add(parameter);
                continue;
            }

            typeName = symbol.DeclaringParameter.Type switch
            {
                NullableTypeSyntax nullableTypeSyntax => ((VariableAccessSyntax)nullableTypeSyntax.Base).Name
                    .IdentifierName,
                VariableAccessSyntax variableAccessSyntax => variableAccessSyntax.Name.IdentifierName,
                _ => typeName
            };

            if (symbol.DeclaringParameter.Type is VariableAccessSyntax || symbol.DeclaringParameter.Type is NullableTypeSyntax)
            {
                parameter.IsUserDefinedType = model.Root.TypeDeclarations.Any(x => x.Name == typeName)
                                              || model.Root.ImportedTypes.Any(x => x.Name == typeName);

                if (parameter.IsUserDefinedType)
                {
                    parameter.Type = typeName;
                    parameter.AllowedValues = null;
                    parameter.IsComplexAllow = false;
                }
            }
            else
            {
                parameter.IsUserDefinedType = false;
            }

            parameter.MaxLength = GetDecorator(symbol, LanguageConstants.ParameterMaxLengthPropertyName);
            parameter.MinLength = GetDecorator(symbol, LanguageConstants.ParameterMinLengthPropertyName);
            parameter.Secure = HasDecorator(symbol, LanguageConstants.ParameterSecurePropertyName);
            parameter.MinValue = GetDecorator(symbol, LanguageConstants.ParameterMinValuePropertyName);
            parameter.MaxValue = GetDecorator(symbol, LanguageConstants.ParameterMaxValuePropertyName);


            var defaultValueSyntaxBase = GetDefaultValue(symbol);
            if (defaultValueSyntaxBase != null)
            {
                parameter.DefaultValue = defaultValueSyntaxBase.ToString();
                parameter.IsComplexDefault = defaultValueSyntaxBase switch
                {
                    ObjectSyntax objectSyntax when objectSyntax.ToNamedPropertyDictionary().IsEmpty => false,
                    ObjectSyntax => true,
                    ArraySyntax arraySyntax => IsComplexArray(arraySyntax),
                    _ => false
                };

                parameter.IsInterpolated = defaultValueSyntaxBase switch
                {
                    StringSyntax stringSyntax when stringSyntax.IsInterpolated() => true,
                    PropertyAccessSyntax => true,
                    _ => false
                };
            }


            model.Root.TypeDeclarations.Any(x => x.Name == templateParameter.Value.TypeReference.Type.Name);


            parameters.Add(parameter);
        }

        return parameters.ToImmutableList();
    }

    private static bool HasDecorator(ParameterSymbol parameterSymbol, string decoratorName)
    {
        var f = parameterSymbol.DeclaringParameter.Decorators;
        var fcs = f.FirstOrDefault(x => (x.Expression as FunctionCallSyntax)?.Name?.IdentifierName == decoratorName);
        return fcs != null;
    }

    private static ParameterSymbol? GetParameterSymbol(SemanticModel model, string paramName) =>
        model.Root.ParameterDeclarations.FirstOrDefault(x => x.Name == paramName);

    private static SyntaxBase? GetDefaultValue(ParameterSymbol parameterSymbol)
    {
        var declaration = parameterSymbol.DeclaringParameter.Modifier as ParameterDefaultValueSyntax;
        return declaration?.DefaultValue;
    }

    private static int? GetDecorator(ParameterSymbol parameterSymbol, string decoratorName)
    {
        var f = parameterSymbol.DeclaringParameter.Decorators;
        var fcs = f.FirstOrDefault(x => (x.Expression as FunctionCallSyntax)?.Name?.IdentifierName == decoratorName);
        var literalText = (fcs?.Arguments.FirstOrDefault()?.Expression as IntegerLiteralSyntax)?.Literal?.Text;
        return int.TryParse(literalText, out var value) ? value : default(int?);
    }

    private static bool IsComplexArray(ArraySyntax syntax)
    {
        if (!syntax.Items.Any())
            return false;

        if (syntax.Items.Count() > 2)
            return true;

        return syntax.Items.First().Value switch
        {
            BooleanLiteralSyntax => false,
            IntegerLiteralSyntax => false,
            StringSyntax => false,
            ArraySyntax => true,
            ObjectSyntax => true,
            _ => true
        };
    }
}