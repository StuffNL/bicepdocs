using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Text;
using LandingZones.Tools.BicepDocs.Core.Models.Parsing;
using LandingZones.Tools.BicepDocs.Core.Services;

namespace LandingZones.Tools.BicepDocs.Formatter.Markdown;

public static class CodeGenerator
{
    public static string GetBicepExample(string moduleName, string moduleAlias, string moduleType, string path,
        string moduleVersion,
        IImmutableList<ParsedParameter> parameters,
        ImmutableList<ParsedUserDefinedType> userDefinedTypes)
    {
        var sb = new StringBuilder();
        sb.AppendLine();

        foreach (var defaultParameter in parameters)
        {
            if (!defaultParameter.IsUserDefinedType)
            {
                sb.AppendLine($"    {defaultParameter.Name}: {GetDefaultValue(defaultParameter)}");
            }
            else
            {
                sb.AppendLine($"    {defaultParameter.Name}: {{");

                var userDefinedType = userDefinedTypes.FirstOrDefault(x => x.Name == defaultParameter.Type);
                foreach (var property in userDefinedType.Properties)
                {
                    sb.AppendLine($"        {property.Name}: {property.Name}");
                }

                sb.AppendLine($"    }}");
            }
        }

        var s = sb.ToString();
        var example = $@"module {moduleName} '{moduleType}/{moduleAlias}:{path}:{moduleVersion}' = {{
  name: '{moduleName}'
  params: {{{s}}}
}}";

        return BicepFormatter.FormatBicepCode(example);
    }

    private static string GetDefaultValue(ParsedParameter parameter)
    {

        if (string.IsNullOrEmpty(parameter.DefaultValue))
        {
            return parameter.Type == "string" ? $"'{parameter.Name}'" : parameter.Name;
        }

        if (parameter.IsUserDefinedType)
        {
            return parameter.Type;
        }

        if (parameter.Type != "string")
        {
            return parameter.DefaultValue;
        }

        if (parameter.DefaultValue.StartsWith('\'') && parameter.DefaultValue.EndsWith('\'') ||
            parameter.IsInterpolated)
        {
            return parameter.DefaultValue;
        }

        return $"'{parameter.DefaultValue}'";
    }
}