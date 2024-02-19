using Azure.Deployments.Core.Definitions.Schema;
using Bicep.Core.Semantics;
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

        var simpleProperty = userDefinedType.Properties.First(x => x.Name == "simpleProperty");
        Assert.AreEqual("simpleProperty", simpleProperty.Name);
        Assert.AreEqual("string", simpleProperty.Type);
        Assert.IsTrue(simpleProperty.IsRequired);

        var complexProperty = userDefinedType.Properties.First(x => x.Name == "complexProperty");
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

        var simpleProperty = userDefinedType.Properties.First(x => x.Name == "simpleProperty");
        Assert.AreEqual("simpleProperty", simpleProperty.Name);
        Assert.AreEqual("string", simpleProperty.Type);
        Assert.IsFalse(simpleProperty.IsRequired);

        var complexProperty = userDefinedType.Properties.First(x => x.Name == "complexProperty");
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


    [TestMethod]
    public async Task Test_ImportType()
    {
        var semanticModel = await GetModel(
            ("main.bicep", """
                           import {exportType} from 'import.bicep'
                           """),
            ("import.bicep", """
                          @export()
                          @description('This is an exportType')
                          type exportType = {
                            //@description('This is a boolean type')
                            //booleanProperty: bool
                            //stringProperty: string?
                            //literalType: 'dev' | 'prd'
                            //@minValue(1)
                            //@maxValue(10)
                            //intType: int
                            //@minLength(1)
                            //@maxLength(5)
                            //stringConstraint: string?
                            @secure()
                            secureString: string
                            customTypeProperty: myIntLiteralType
                          }
                          
                          type myIntLiteralType = {
                            stringProp: string
                          }
                          """
        ));

        var userDefinedTypes = ImportTypeParser.ParseImportTypes(semanticModel);
        var userDefinedType = userDefinedTypes.First(x => x.Name == "exportType");
        Assert.AreEqual("exportType", userDefinedType.Name);

        var customTypeProperty = userDefinedType.Properties.First(x => x.Name == "customTypeProperty");
        Assert.AreEqual("myIntLiteralType", customTypeProperty.Type);

    }

}
