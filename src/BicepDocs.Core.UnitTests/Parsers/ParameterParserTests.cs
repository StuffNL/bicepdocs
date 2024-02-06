using LandingZones.Tools.BicepDocs.Core.Parsers;

namespace LandingZones.Tools.BicepDocs.Core.UnitTests.Parsers;

[TestClass]
public class ParameterParserTests : BicepFileTestBase
{
    [TestMethod]
    public async Task Parameter_Basic_Parses()
    {
        const string template = @"targetScope = 'subscription'

@description('Name of the resource group')
param resourceGroupName string

param resourceGroupLocation string

@description('Tags to append to resource group')
param tags object = {}

resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-01-01' = {
  name: resourceGroupName
  location: resourceGroupLocation
  tags: tags
}";
        var semanticModel = await GetModel(template);
        var parameters = ParameterParser.ParseParameters(semanticModel);

        Assert.AreEqual(3, parameters.Count);

        var tags = parameters.First(x => x.Name == "tags");
        Assert.IsFalse(tags.IsComplexAllow);
        Assert.IsFalse(tags.IsComplexDefault);
        Assert.AreEqual("tags", tags.Name);
        Assert.AreEqual("object", tags.Type);
        Assert.AreEqual("{}", tags.DefaultValue);
        Assert.AreEqual("Tags to append to resource group", tags.Description);

        var resourceGroupName = parameters.First(x => x.Name == "resourceGroupName");
        Assert.IsTrue(resourceGroupName.IsRequired);
    }


    [TestMethod]
    public async Task Parameter_Constraints_Parses()
    {
        const string template = @"targetScope = 'subscription'

@minLength(3)
@maxLength(24)
@description('Name of the resource group')
param resourceGroupName string

@minLength(3)
@maxLength(24)
param someArr array = []

@maxValue(100)
@minValue(1)
param intVal int = 1

@secure()
param objectVal object = {}

resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-01-01' = {
  name: resourceGroupName
}";
        var semanticModel = await GetModel(template);
        var parameters = ParameterParser.ParseParameters(semanticModel);

        var resourceGroupName = parameters.First(x => x.Name == "resourceGroupName");
        Assert.AreEqual("resourceGroupName", resourceGroupName.Name);
        Assert.AreEqual("string", resourceGroupName.Type);
        Assert.AreEqual("Name of the resource group", resourceGroupName.Description);
        Assert.AreEqual(3, resourceGroupName.MinLength);
        Assert.AreEqual(24, resourceGroupName.MaxLength);


        var someArr = parameters.First(x => x.Name == "someArr");
        Assert.AreEqual("someArr", someArr.Name);
        Assert.AreEqual("array", someArr.Type);
        Assert.AreEqual(3, someArr.MinLength);
        Assert.AreEqual(24, someArr.MaxLength);


        var intVal = parameters.First(x => x.Name == "intVal");
        Assert.AreEqual("intVal", intVal.Name);
        Assert.AreEqual("int", intVal.Type);
        Assert.AreEqual(100, intVal.MaxValue);
        Assert.AreEqual(1, intVal.MinValue);

        var objectVal = parameters.First(x => x.Name == "objectVal");
        Assert.AreEqual("objectVal", objectVal.Name);
        Assert.AreEqual("object", objectVal.Type);
        Assert.IsTrue(objectVal.Secure);
    }

    [TestMethod]
    public async Task Parameter_Interpolated_Parses()
    {
        const string template = @"targetScope = 'subscription'

@description('Name of the resource group')
param resourceGroupName string

param resourceGroupLocation string = resourceGroup().location

resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-01-01' = {
  name: resourceGroupName
  location: resourceGroupLocation
}";
        var semanticModel = await GetModel(template);
        var parameters = ParameterParser.ParseParameters(semanticModel);

        var resourceGroupLocation = parameters.First(x => x.Name == "resourceGroupLocation");
        Assert.IsFalse(resourceGroupLocation.IsComplexAllow);
        Assert.IsTrue(resourceGroupLocation.IsInterpolated);
        Assert.AreEqual("resourceGroupLocation", resourceGroupLocation.Name);
        Assert.AreEqual("string", resourceGroupLocation.Type);
        Assert.AreEqual("resourceGroup().location", resourceGroupLocation.DefaultValue);
    }

    [TestMethod]
    public async Task Parameter_String_Parses()
    {
        const string template = @"param stringParam string = 'string-value'
resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-01-01' = {
  name: resourceGroupName
  location: resourceGroupLocation
  tags: tags
}";
        var semanticModel = await GetModel(template);
        var parameters = ParameterParser.ParseParameters(semanticModel);

        var param = parameters.First(x => x.Name == "stringParam");
        Assert.IsFalse(param.IsComplexAllow);
        Assert.IsFalse(param.IsComplexDefault);
        Assert.AreEqual("stringParam", param.Name);
        Assert.AreEqual("string", param.Type);
        Assert.AreEqual("'string-value'", param.DefaultValue);
        Assert.IsFalse(param.IsRequired);
    }

    [TestMethod]
    public async Task Parameter_Bool_Parses()
    {
        const string template = @"param boolParam bool = true
resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-01-01' = {
  name: resourceGroupName
  location: resourceGroupLocation
  tags: tags
}";
        var semanticModel = await GetModel(template);
        var parameters = ParameterParser.ParseParameters(semanticModel);

        var param = parameters.First(x => x.Name == "boolParam");
        Assert.IsFalse(param.IsComplexAllow);
        Assert.IsFalse(param.IsComplexDefault);
        Assert.AreEqual("boolParam", param.Name);
        Assert.AreEqual("bool", param.Type);
        Assert.AreEqual("true", param.DefaultValue);
        Assert.IsFalse(param.IsRequired);
    }

    [TestMethod]
    public async Task Parameter_SimpleObject_Parses()
    {
        const string template = @"param objectParam object = {}
resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-01-01' = {
  name: resourceGroupName
  location: resourceGroupLocation
  tags: tags
}";
        var semanticModel = await GetModel(template);
        var parameters = ParameterParser.ParseParameters(semanticModel);

        var param = parameters.First(x => x.Name == "objectParam");
        Assert.IsFalse(param.IsComplexAllow);
        Assert.IsFalse(param.IsComplexDefault);
        Assert.AreEqual("objectParam", param.Name);
        Assert.AreEqual("object", param.Type);
        Assert.AreEqual("{}", param.DefaultValue);
        Assert.IsFalse(param.IsRequired);
    }

    [TestMethod]
    public async Task Parameter_ComplexObject_Parses()
    {
        const string template = @"param objectParam object = {
name: 'hello'
}
resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-01-01' = {
  name: resourceGroupName
  location: resourceGroupLocation
  tags: tags
}";
        var semanticModel = await GetModel(template);
        var parameters = ParameterParser.ParseParameters(semanticModel);

        var param = parameters.First(x => x.Name == "objectParam");
        Assert.IsFalse(param.IsComplexAllow);
        Assert.IsTrue(param.IsComplexDefault);
        Assert.AreEqual("objectParam", param.Name);
        Assert.AreEqual("object", param.Type);
        Assert.IsFalse(param.IsRequired);
        Assert.AreEqual(@"{
name: 'hello'
}", param.DefaultValue);
    }

    [TestMethod]
    public async Task Parameter_SimpleArray_Parses()
    {
        const string template = @"param arrayParam array = []
resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-01-01' = {
  name: resourceGroupName
  location: resourceGroupLocation
  tags: tags
}";
        var semanticModel = await GetModel(template);
        var parameters = ParameterParser.ParseParameters(semanticModel);

        var param = parameters.First(x => x.Name == "arrayParam");
        Assert.IsFalse(param.IsComplexAllow);
        Assert.IsFalse(param.IsComplexDefault);
        Assert.AreEqual("arrayParam", param.Name);
        Assert.AreEqual("array", param.Type);
        Assert.AreEqual("[]", param.DefaultValue);
        Assert.IsFalse(param.IsRequired);
    }

    [DataTestMethod]
    [DataRow(false, true)]
    [DataRow(1, false)]
    [DataRow("'hello'", false)]
    [DataRow("{one: 'two'}", true)]
    [DataRow("[{one: 'two'}]", true)]
    public async Task Parameter_ArrayWithSingleItem_Parses(object defaultValue, bool isComplex)
    {
        var template = @$"param arrayParam array = [
{defaultValue}
]
resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-01-01' = {{
  name: resourceGroupName
  location: resourceGroupLocation
  tags: tags
}}";
        var semanticModel = await GetModel(template);
        var parameters = ParameterParser.ParseParameters(semanticModel);

        var param = parameters.First(x => x.Name == "arrayParam");
        Assert.IsFalse(param.IsComplexAllow);
        Assert.AreEqual(isComplex, param.IsComplexDefault);
        Assert.AreEqual("arrayParam", param.Name);
        Assert.AreEqual("array", param.Type);
        Assert.AreEqual(@$"[
{defaultValue}
]", param.DefaultValue);
    }

    [TestMethod]
    public async Task Parameter_ComplexArray_Parses()
    {
        const string template = @"param arrayParam array = [
'one'
'two'
'three'
]
resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-01-01' = {
  name: resourceGroupName
  location: resourceGroupLocation
  tags: tags
}";
        var semanticModel = await GetModel(template);
        var parameters = ParameterParser.ParseParameters(semanticModel);

        var param = parameters.First(x => x.Name == "arrayParam");
        Assert.IsFalse(param.IsComplexAllow);
        Assert.IsTrue(param.IsComplexDefault);
        Assert.AreEqual("arrayParam", param.Name);
        Assert.AreEqual("array", param.Type);
        Assert.IsFalse(param.IsRequired);
        Assert.AreEqual(@"[
'one'
'two'
'three'
]", param.DefaultValue);
    }

    [TestMethod]
    public async Task Parameter_AllowedValues_Parses()
    {
        const string template = @"
@allowed([
'one'
'two'
'three'
])
param stringParam string = 'string-value'

resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-01-01' = {
  name: resourceGroupName
  location: resourceGroupLocation
  tags: tags
}";
        var semanticModel = await GetModel(template);
        var parameters = ParameterParser.ParseParameters(semanticModel);

        var param = parameters.First(x => x.Name == "stringParam");
        Assert.IsTrue(param.IsComplexAllow);
        Assert.IsFalse(param.IsComplexDefault);
        Assert.IsNotNull(param.AllowedValues);
        Assert.AreEqual(3, param.AllowedValues.Count);
        Assert.AreEqual("stringParam", param.Name);
        Assert.AreEqual("'one' | 'three' | 'two'", param.Type);
        Assert.AreEqual("'string-value'", param.DefaultValue);
        Assert.IsFalse(param.IsRequired);
    }


    [TestMethod]
    public async Task Parameter_UserDefined_Parses()
    {
        const string template = @"
type myType = {
    stringProp: string
    intProp: int
    intProp2: boolean
}

param typeParam myType
";
        var semanticModel = await GetModel(template);
        var parameters = ParameterParser.ParseParameters(semanticModel);

        var param = parameters.First(x => x.Name == "typeParam");
        Assert.IsFalse(param.IsComplexAllow);
        Assert.IsTrue(param.IsUserDefinedType);
        Assert.IsNull(param.AllowedValues);
        Assert.AreEqual(param.Type, "myType");
        Assert.IsTrue(param.IsRequired);

    }

    public async Task Parameter_UserDefined_Optionally_Parses()
    {
        const string template = @"
type myType = {
    stringProp: string
    intProp: int
    intProp2: boolean
}

param typeParam myType?
";
        var semanticModel = await GetModel(template);
        var parameters = ParameterParser.ParseParameters(semanticModel);

        var param = parameters.First(x => x.Name == "typeParam");
        Assert.IsFalse(param.IsComplexAllow);
        Assert.IsTrue(param.IsUserDefinedType);
        Assert.IsNull(param.AllowedValues);
        Assert.AreEqual(param.Type, "myType");
        Assert.IsFalse(param.IsRequired);

    }

    [TestMethod]
    public async Task Parameter_ComplexUserDefined_Parses()
    {
        const string template = @"
type myType = {
  stringProp: string
  arrayProp: string[]
  intProp: int
  intProp2: int  
  timeGrain: 'Annually' | 'BillingAnnual' | 'BillingMonth' | 'BillingQuarter' | 'Monthly' | 'Quarterly'
}

@description('This is the myType parameter')
param typeParam myType
";
        var semanticModel = await GetModel(template);
        var parameters = ParameterParser.ParseParameters(semanticModel);

        var param = parameters.First(x => x.Name == "typeParam");
        Assert.IsFalse(param.IsComplexAllow);
        Assert.IsTrue(param.IsUserDefinedType);
        Assert.IsNull(param.AllowedValues);
        Assert.AreEqual(param.Type, "myType");
        Assert.IsTrue(param.IsRequired);

    }

    [TestMethod]
    public async Task Parameter_ComplexUserDefined_Optionally_Parses()
    {
        const string template = @"
type myType = {
  stringProp: string
  arrayProp: string[]
  intProp: int
  intProp2: int  
  timeGrain: 'Annually' | 'BillingAnnual' | 'BillingMonth' | 'BillingQuarter' | 'Monthly' | 'Quarterly'
}

@description('This is the myType parameter')
param typeParam myType?
";
        var semanticModel = await GetModel(template);
        var parameters = ParameterParser.ParseParameters(semanticModel);

        var param = parameters.First(x => x.Name == "typeParam");
        Assert.IsFalse(param.IsComplexAllow);
        Assert.IsTrue(param.IsUserDefinedType);
        Assert.IsNull(param.AllowedValues);
        Assert.AreEqual(param.Type, "myType");
        Assert.IsFalse(param.IsRequired);
    }
}