using System.Collections.Immutable;
using LandingZones.Tools.BicepDocs.Core;
using LandingZones.Tools.BicepDocs.Core.Models.Parsing;
using LandingZones.Tools.BicepDocs.Core.Parsers;
using LandingZones.Tools.BicepDocs.Formatter.Markdown.Elements;
using LandingZones.Tools.BicepDocs.Formatter.Markdown.Extensions;

namespace LandingZones.Tools.BicepDocs.Formatter.Markdown.Generators;

internal static class UserDefinedTypeGenerator
{
    internal static void BuildUserDefinedTypes(MarkdownDocument document, IImmutableList<ParsedUserDefinedType> types)
    {
        document.Append(new MkHeader("User Defined Types", MkHeaderLevel.H2));

        foreach (var type in types)
        {
            document.Append(new MkHeader(type.Name, MkHeaderLevel.H3));
            var typeOverviewTable = new MkTable().AddColumn("Member").AddColumn("Description").AddColumn("Type")
                .AddColumn("Default");

            foreach (var property in type.Properties)
            {
                typeOverviewTable.AddRow(property.Name.WrapInBackticks(), property.Description ?? "", property.Type, property.DefaultValue);
            }

            document.Append(typeOverviewTable);
        }
    }

    internal static void BuildUserDefinedTypes(MarkdownDocument document, FormatterContext context)
    {
        if (!context.FormatterOptions.IncludeOutputs) return;
        var types = UserDefinedTypeParser.ParseUserDefinedTypes(context.Template);
        if (!types.Any()) return;
        BuildUserDefinedTypes(document, types);
    }
}

