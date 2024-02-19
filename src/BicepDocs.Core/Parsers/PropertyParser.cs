using Bicep.Core;
using Bicep.Core.TypeSystem;
using Bicep.Core.TypeSystem.Types;
using LandingZones.Tools.BicepDocs.Core.Models.Parsing;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace LandingZones.Tools.BicepDocs.Core.Parsers
{
    internal class PropertyParser
    {
        public static Dictionary<string, string> GetPropertyTypes(ObjectType objectType)
        {
            var objectTypeName = Regex.Replace(objectType.Name, "[{}?]", "").Replace("null |", "");

            var propertyTypes = objectTypeName.Split(',').Select(x => new
            {
                Name = x.Split(':').First().Trim(),
                Type = x.Split(':').Last().Trim()
            }).ToDictionary(x => x.Name, x => x.Type);
            return propertyTypes;
        }

        public static List<ParsedUserDefinedTypeProperty> ParseProperties(ImmutableSortedDictionary<string, TypeProperty> properties, Dictionary<string, string> propertyTypes)
        {
            var parsedProperties = new List<ParsedUserDefinedTypeProperty>();

            foreach (var property in properties)
            {
                var userDefinedTypeProperty =
                    new ParsedUserDefinedTypeProperty(property.Key, propertyTypes[property.Key])
                    {
                        Description = property.Value.Description,
                    };

                if (property.Value.TypeReference.Type is UnionType unionType)
                {
                    if (unionType.Members.All(x => x.Type.TypeKind == TypeKind.StringLiteral))
                    {
                        var allowList = unionType.Members.ToArray();
                        var allowValues = allowList.Select(x => x.Type.Name).ToList();
                        userDefinedTypeProperty.IsComplexAllow = allowList.Length > 2;
                        userDefinedTypeProperty.AllowedValues = allowValues;
                    }
                    else
                    {
                        foreach (var member in unionType.Members)
                        {
                            if (member.Type.Name == LanguageConstants.NullKeyword)
                            {
                                userDefinedTypeProperty.IsRequired = false;
                                continue;
                            }

                            userDefinedTypeProperty = ParseTypeProperty(member, userDefinedTypeProperty);
                        }
                    }
                }
                else
                {
                    userDefinedTypeProperty = ParseTypeProperty(property.Value.TypeReference.Type, userDefinedTypeProperty);
                }

                userDefinedTypeProperty.Type = propertyTypes[property.Key];
                parsedProperties.Add(userDefinedTypeProperty);
            }

            return parsedProperties;
        }

        private static ParsedUserDefinedTypeProperty ParseTypeProperty(ITypeReference type,
            ParsedUserDefinedTypeProperty userDefinedTypeProperty)
        {
            switch (type.Type)
            {
                case IntegerType integerType:
                    userDefinedTypeProperty.MinValue = integerType.MinValue.HasValue ? (int)integerType.MinValue : null;
                    userDefinedTypeProperty.MaxValue = integerType.MaxValue.HasValue ? (int)integerType.MaxValue : null;
                    break;
                case StringType stringType:
                    userDefinedTypeProperty.MinLength = stringType.MinLength.HasValue ? (int)stringType.MinLength : null;
                    userDefinedTypeProperty.MaxLength = stringType.MaxLength.HasValue ? (int)stringType.MaxLength : null;
                    break;
            }

            userDefinedTypeProperty.Secure = (type.Type.ValidationFlags & TypeSymbolValidationFlags.IsSecure) != 0;
            userDefinedTypeProperty.Type = type.Type.Name;

            return userDefinedTypeProperty;
        }
    }
}
