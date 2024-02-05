using LandingZones.Tools.BicepDocs.Core.Parsers;

namespace LandingZones.Tools.BicepDocs.Core.UnitTests.Parsers;

[TestClass]
public class UserDefinedTypeParserTests : BicepFileTestBase
{


    [TestMethod]
    public async Task Type_Constraints_Parses()
    {
        const string template = @"

@description('This is the budgetType')
type budgetType = {
  name: string
}";
        var semanticModel = await GetModel(template);
        var userDefinedTypes = UserDefinedTypeParser.ParseUserDefinedTypes(semanticModel);

        var userDefinedType = userDefinedTypes.First(x => x.Name == "budgetType");
        Assert.AreEqual("budgetType", userDefinedType.Name);
        Assert.AreEqual("This is the budgetType", userDefinedType.Description);
        Assert.AreEqual(1, userDefinedType.Properties.Count);

    }


    [TestMethod]
    public async Task Type_Parameter_Constraints_Parses()
    {
        const string template = @"
type budgetType = {
  @minValue(1)
  @maxValue(10)
  @description('This is threshold 3')
  threshold3: int
}";
        var semanticModel = await GetModel(template);
        var userDefinedTypes = UserDefinedTypeParser.ParseUserDefinedTypes(semanticModel);

        var userDefinedType = userDefinedTypes.First(x => x.Name == "budgetType");
        Assert.AreEqual("budgetType", userDefinedType.Name);
        Assert.AreEqual("", userDefinedType.Description);
        Assert.AreEqual(1, userDefinedType.Properties.Count);

        var firstProperty = userDefinedType.Properties.First();
        Assert.AreEqual(1, firstProperty.MinValue);
        Assert.AreEqual(10, firstProperty.MaxValue);
        Assert.AreEqual("int", firstProperty.Type);
        Assert.AreEqual("This is threshold 3", firstProperty.Description);
    }


    [TestMethod]
    public async Task Type_Parameter_Allowed_Parses()
    {
        const string template = @"
type budgetType = {
  timeGrain: 'Annually' | 'BillingAnnual' | 'BillingMonth' | 'BillingQuarter' | 'Monthly' | 'Quarterly'
}";
        var semanticModel = await GetModel(template);
        var userDefinedTypes = UserDefinedTypeParser.ParseUserDefinedTypes(semanticModel);

        var userDefinedType = userDefinedTypes.First(x => x.Name == "budgetType");
        Assert.AreEqual("budgetType", userDefinedType.Name);
        Assert.AreEqual("", userDefinedType.Description);
        Assert.AreEqual(1, userDefinedType.Properties.Count);

        var firstProperty = userDefinedType.Properties.First();
        Assert.IsTrue(firstProperty.IsComplexAllow);
        Assert.IsFalse(firstProperty.IsComplexDefault);
        Assert.IsNotNull(firstProperty.AllowedValues);
        Assert.AreEqual(6, firstProperty.AllowedValues.Count);
        Assert.AreEqual("timeGrain", firstProperty.Name);
        Assert.AreEqual("'Annually' | 'BillingAnnual' | 'BillingMonth' | 'BillingQuarter' | 'Monthly' | 'Quarterly'", firstProperty.Type);
    }
}