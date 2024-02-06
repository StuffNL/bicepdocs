using System.Collections.Immutable;
using LandingZones.Tools.BicepDocs.Core.Models.Formatting;
using LandingZones.Tools.BicepDocs.Core.Models.Parsing;
using LandingZones.Tools.BicepDocs.Core.UnitTests;
using LandingZones.Tools.BicepDocs.Formatter.Markdown.Elements;
using LandingZones.Tools.BicepDocs.Formatter.Markdown.Generators;

namespace LandingZones.Tools.BicepDocs.Formatter.Markdown.UnitTests.Generators;

[TestClass]
public class UserDefinedTypeGeneratorTests
{
    #region UserDefinedTypes
    [TestMethod]
    public void BuildParameters_UserDefinedTypes_WithoutDescription()
    {
        var expected = @"## User Defined Types

### myType

| Property | Description | Type | Required |
| --- | --- | --- | --- |
| `stringProperty` | Some description | string | True |".ToPlatformLineEndings() +
                       Environment.NewLine;

        var userDefinedTypes = new List<ParsedUserDefinedType>
        {
            new ParsedUserDefinedType("myType", new List<ParsedUserDefinedTypeProperty>
            {
                new ParsedUserDefinedTypeProperty("stringProperty", "string")
                {
                    Description = "Some description"
                }
            })
        }.ToImmutableList();
        var document = new MarkdownDocument();

        UserDefinedTypeGenerator.BuildUserDefinedTypes(document, userDefinedTypes);

        Assert.AreEqual(3, document.Count);

        var md = document.ToMarkdown();

        Assert.AreEqual(expected, md);
    }


    [TestMethod]
    public void BuildParameters_UserDefinedTypes_WithDescription()
    {
        var expected = @"## User Defined Types

### myType

> Type description

| Property | Description | Type | Required |
| --- | --- | --- | --- |
| `stringProperty` | Some description | string | True |".ToPlatformLineEndings() +
                       Environment.NewLine;

        var userDefinedTypes = new List<ParsedUserDefinedType>
        {
            new("myType", new List<ParsedUserDefinedTypeProperty>
            {
                new("stringProperty", "string")
                {
                    Description = "Some description"
                }
            })
            {
                Description = "Type description"
            }
        }.ToImmutableList();
        var document = new MarkdownDocument();

        UserDefinedTypeGenerator.BuildUserDefinedTypes(document, userDefinedTypes);

        Assert.AreEqual(4, document.Count);

        var md = document.ToMarkdown();

        Assert.AreEqual(expected, md);
    }

    [TestMethod]
    public void BuildParameters_UserDefinedTypes_OptionalProperty()
    {
        var expected = @"## User Defined Types

### myType

| Property | Description | Type | Required |
| --- | --- | --- | --- |
| `stringProperty` | Some description | string | False |".ToPlatformLineEndings() +
                       Environment.NewLine;

        var userDefinedTypes = new List<ParsedUserDefinedType>
        {
            new ParsedUserDefinedType("myType", new List<ParsedUserDefinedTypeProperty>
            {
                new ParsedUserDefinedTypeProperty("stringProperty", "string")
                {
                    Description = "Some description",
                    IsRequired = false
                }
            })
        }.ToImmutableList();
        var document = new MarkdownDocument();

        UserDefinedTypeGenerator.BuildUserDefinedTypes(document, userDefinedTypes);

        Assert.AreEqual(3, document.Count);

        var md = document.ToMarkdown();

        Assert.AreEqual(expected, md);
    }
    #endregion
}