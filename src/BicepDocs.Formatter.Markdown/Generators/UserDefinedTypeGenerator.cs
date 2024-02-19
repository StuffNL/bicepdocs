using System.Collections.Immutable;
using LandingZones.Tools.BicepDocs.Core;
using LandingZones.Tools.BicepDocs.Core.Models.Parsing;
using LandingZones.Tools.BicepDocs.Core.Parsers;
using LandingZones.Tools.BicepDocs.Formatter.Markdown.Elements;
using LandingZones.Tools.BicepDocs.Formatter.Markdown.Extensions;

namespace LandingZones.Tools.BicepDocs.Formatter.Markdown.Generators;

internal static class UserDefinedTypeGenerator
{
    internal static void BuildUserDefinedTypes(MarkdownDocument document, IImmutableList<ParsedUserDefinedType> userDefinedTypes)
    {
        document.Append(new MkHeader("User Defined Types", MkHeaderLevel.H2));

        foreach (var userDefinedType in userDefinedTypes)
        {
            document.Append(new MkHeader(userDefinedType.Name, MkHeaderLevel.H3));

            if (!string.IsNullOrEmpty(userDefinedType.Description))
            {
                document.Append(new MkBlockQuote(userDefinedType.Description));
            }

            if (!userDefinedType.IsPrimitiveLiteral)
            {
                var typeOverviewTable = new MkTable().AddColumn("Property").AddColumn("Description").AddColumn("Type")
                    .AddColumn("Required");

                foreach (var property in userDefinedType.Properties)
                {
                    var type = BuildType(property);
                    typeOverviewTable.AddRow(property.Name.WrapInBackticks(), property.Description ?? "", type,
                        property.IsRequired.ToString());
                }
                document.Append(typeOverviewTable);
            }
        }
    }

    internal static void BuildUserDefinedTypes(MarkdownDocument document, FormatterContext context)
    {
        if (!context.FormatterOptions.IncludeOutputs) return;
        var types = UserDefinedTypeParser.ParseUserDefinedTypes(context.Template);
        var importedUserDefinedTypes = ImportTypeParser.ParseImportTypes(context.Template);
        if (!types.Any() && !importedUserDefinedTypes.Any()) return;
        BuildUserDefinedTypes(document, types.AddRange(importedUserDefinedTypes));
    }



    private static string BuildType(ParsedUserDefinedTypeProperty property)
    {
        var type = string.Empty;


        type += property.Type.Replace("|", "\\|");
        if (property.Secure) type += " (secure)";


        var minMax = GetCharacterLimit(property);
        if (minMax != null)
        {
            type += " <br/> <br/>";
            type += $"Character limit: {minMax}";
        }

        var value = GetAcceptedValues(property);
        if (value != null)
        {
            type += " <br/> <br/>";
            type += $"Accepted values: {value}";
            ;
        }


        return type;
    }

    public static string? GetAcceptedValues(ParsedUserDefinedTypeProperty parameter)
    {
        if (parameter.MinValue == null && parameter.MaxValue == null)
            return null;

        if (parameter is { MinValue: { }, MaxValue: { } })
        {
            return $"from {parameter.MinValue} to {parameter.MaxValue}.";
        }

        if (parameter.MinValue != null)
        {
            return $"from {parameter.MinValue}.";
        }

        if (parameter.MaxValue != null)
        {
            return $"to {parameter.MaxValue}.";
        }

        return null;
    }


    private static string? GetCharacterLimit(ParsedUserDefinedTypeProperty parameter)
    {
        if (parameter.MinLength == null && parameter.MaxLength == null)
        {
            return null;
        }

        if (parameter is { MinLength: { }, MaxLength: { } })
        {
            return $"{parameter.MinLength}-{parameter.MaxLength}";
        }

        if (parameter.MinLength != null)
        {
            return $"{parameter.MinLength}-X";
        }

        if (parameter.MaxLength != null)
        {
            return $"X-{parameter.MaxLength}";
        }

        return null;
    }
}

