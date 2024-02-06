using LandingZones.Tools.BicepDocs.Core.Parsers;

namespace LandingZones.Tools.BicepDocs.Core.UnitTests.Parsers;

[TestClass]
public class UserDefinedTypeParserTests : BicepFileTestBase
{


    [TestMethod]
    public async Task Type_Constraints_Parses()
    {
        const string template = @"

@description('This is the userDefinedType')
type userDefinedType = {
  stringProp: string
}";
        var semanticModel = await GetModel(template);
        var userDefinedTypes = UserDefinedTypeParser.ParseUserDefinedTypes(semanticModel);

        var userDefinedType = userDefinedTypes.First(x => x.Name == "userDefinedType");
        Assert.AreEqual("userDefinedType", userDefinedType.Name);
        Assert.AreEqual("This is the userDefinedType", userDefinedType.Description);
        Assert.AreEqual(1, userDefinedType.Properties.Count);
        Assert.IsFalse(userDefinedType.IsPrimitiveLiteral);

    }


    [TestMethod]
    public async Task Type_Parameter_Constraints_Parses()
    {
        const string template = @"
type userDefinedType = {
  @minValue(1)
  @maxValue(10)
  @description('This is the intProp description')
  intProp: int
}";
        var semanticModel = await GetModel(template);
        var userDefinedTypes = UserDefinedTypeParser.ParseUserDefinedTypes(semanticModel);

        var userDefinedType = userDefinedTypes.First(x => x.Name == "userDefinedType");
        Assert.AreEqual("userDefinedType", userDefinedType.Name);
        Assert.AreEqual("", userDefinedType.Description);
        Assert.AreEqual(1, userDefinedType.Properties.Count);
        Assert.IsFalse(userDefinedType.IsPrimitiveLiteral);

        var firstProperty = userDefinedType.Properties.First();
        Assert.AreEqual(1, firstProperty.MinValue);
        Assert.AreEqual(10, firstProperty.MaxValue);
        Assert.AreEqual("intProp", firstProperty.Name);
        Assert.AreEqual("int", firstProperty.Type);
        Assert.AreEqual("This is the intProp description", firstProperty.Description);
    }


    [TestMethod]
    public async Task Type_Parameter_Allowed_Parses()
    {
        const string template = @"
type userDefinedType = {
  timeGrain: 'Annually' | 'BillingAnnual' | 'BillingMonth' | 'BillingQuarter' | 'Monthly' | 'Quarterly'
}";
        var semanticModel = await GetModel(template);
        var userDefinedTypes = UserDefinedTypeParser.ParseUserDefinedTypes(semanticModel);

        var userDefinedType = userDefinedTypes.First(x => x.Name == "userDefinedType");
        Assert.AreEqual("userDefinedType", userDefinedType.Name);
        Assert.AreEqual("", userDefinedType.Description);
        Assert.AreEqual(1, userDefinedType.Properties.Count);
        Assert.IsFalse(userDefinedType.IsPrimitiveLiteral);

        var firstProperty = userDefinedType.Properties.First();
        Assert.IsTrue(firstProperty.IsComplexAllow);
        Assert.IsFalse(firstProperty.IsComplexDefault);
        Assert.IsNotNull(firstProperty.AllowedValues);
        Assert.AreEqual(6, firstProperty.AllowedValues.Count);
        Assert.AreEqual("timeGrain", firstProperty.Name);
        Assert.IsTrue(firstProperty.IsRequired);
        Assert.AreEqual("'Annually' | 'BillingAnnual' | 'BillingMonth' | 'BillingQuarter' | 'Monthly' | 'Quarterly'", firstProperty.Type);
    }

    [TestMethod]
    public async Task Type_Parameter_Complex_Property_Parses()
    {
        const string template = @"
type userDefinedType = {
  simpleProperty: string
  complexProperty: complexType
}

type complexType = {
  simpleProperty: string
}
";
        var semanticModel = await GetModel(template);
        var userDefinedTypes = UserDefinedTypeParser.ParseUserDefinedTypes(semanticModel);

        var userDefinedType = userDefinedTypes.First(x => x.Name == "userDefinedType");
        Assert.AreEqual("userDefinedType", userDefinedType.Name);
        Assert.AreEqual("", userDefinedType.Description);
        Assert.AreEqual(2, userDefinedType.Properties.Count);
        Assert.IsFalse(userDefinedType.IsPrimitiveLiteral);

        var simpleProperty = userDefinedType.Properties.First();
        Assert.AreEqual("simpleProperty", simpleProperty.Name);
        Assert.AreEqual("string", simpleProperty.Type);
        Assert.IsTrue(simpleProperty.IsRequired);

        var complexProperty = userDefinedType.Properties.Last();
        Assert.AreEqual("complexProperty", complexProperty.Name);
        Assert.AreEqual("complexType", complexProperty.Type);
        Assert.IsTrue(complexProperty.IsRequired);
    }

    [TestMethod]
    public async Task Type_Parameter_Complex_Property_IsOptional_Parses()
    {
        const string template = @"
type userDefinedType = {
  simpleProperty: string?
  complexProperty: complexType?
}

type complexType = {
  simpleProperty: string?
}
";
        var semanticModel = await GetModel(template);
        var userDefinedTypes = UserDefinedTypeParser.ParseUserDefinedTypes(semanticModel);

        var userDefinedType = userDefinedTypes.First(x => x.Name == "userDefinedType");
        Assert.AreEqual("userDefinedType", userDefinedType.Name);
        Assert.AreEqual("", userDefinedType.Description);
        Assert.AreEqual(2, userDefinedType.Properties.Count);
        Assert.IsFalse(userDefinedType.IsPrimitiveLiteral);

        var simpleProperty = userDefinedType.Properties.First();
        Assert.AreEqual("simpleProperty", simpleProperty.Name);
        Assert.AreEqual("string", simpleProperty.Type);
        Assert.IsFalse(simpleProperty.IsRequired);

        var complexProperty = userDefinedType.Properties.Last();
        Assert.AreEqual("complexProperty", complexProperty.Name);
        Assert.AreEqual("complexType", complexProperty.Type);
        Assert.IsFalse(complexProperty.IsRequired);

    }

    [TestMethod]
    public async Task Type_IsPrimitiveLiteral_String_Parser()
    {
        const string template = @"
type myStringLiteralType = 'dev' | 'tst' | 'acc' | 'prd' | 'shared'
";
        var semanticModel = await GetModel(template);
        var userDefinedTypes = UserDefinedTypeParser.ParseUserDefinedTypes(semanticModel);

        var userDefinedType = userDefinedTypes.First(x => x.Name == "myStringLiteralType");
        Assert.AreEqual("myStringLiteralType", userDefinedType.Name);
        Assert.AreEqual("'dev' | 'tst' | 'acc' | 'prd' | 'shared'", userDefinedType.Description);
        Assert.AreEqual(0, userDefinedType.Properties.Count);
        Assert.IsTrue(userDefinedType.IsPrimitiveLiteral);
    }

    [TestMethod]
    public async Task Type_IsPrimitiveLiteral_Int_Parser()
    {
        const string template = @"
type myIntLiteralType = 10
";
        var semanticModel = await GetModel(template);
        var userDefinedTypes = UserDefinedTypeParser.ParseUserDefinedTypes(semanticModel);

        var userDefinedType = userDefinedTypes.First(x => x.Name == "myIntLiteralType");
        Assert.AreEqual("myIntLiteralType", userDefinedType.Name);
        Assert.AreEqual("10", userDefinedType.Description);
        Assert.AreEqual(0, userDefinedType.Properties.Count);
        Assert.IsTrue(userDefinedType.IsPrimitiveLiteral);
    }

    [TestMethod]
    public async Task Type_IsPrimitiveLiteral_String_Single_Parser()
    {
        const string template = @"
type myStringLiteralType = 'single'
";
        var semanticModel = await GetModel(template);
        var userDefinedTypes = UserDefinedTypeParser.ParseUserDefinedTypes(semanticModel);

        var userDefinedType = userDefinedTypes.First(x => x.Name == "myStringLiteralType");
        Assert.AreEqual("myStringLiteralType", userDefinedType.Name);
        Assert.AreEqual("'single'", userDefinedType.Description);
        Assert.AreEqual(0, userDefinedType.Properties.Count);
        Assert.IsTrue(userDefinedType.IsPrimitiveLiteral);
    }
}
