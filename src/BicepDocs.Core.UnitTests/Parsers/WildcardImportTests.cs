using Azure.Deployments.Core.Definitions.Schema;
using Bicep.Core.Semantics;
using LandingZones.Tools.BicepDocs.Core.Parsers;

namespace LandingZones.Tools.BicepDocs.Core.UnitTests.Parsers;

[TestClass]
public class WildcardImportTests : BicepFileTestBase
{
    [TestMethod]
    [Ignore]
    public async Task Test_ImportedTypesImplicit()
    {
        var semanticModel = await GetModel(
            ("main.bicep", """
                           import * as userDefinedTypes from 'import.bicep'
                           
                           param test userDefinedTypes.exportType
                           """),
            ("import.bicep", """
                             @export()
                             @description('This is an exportType')
                             type exportType = {
                               @description('This is a boolean type')
                               booleanProperty: bool
                             }
                             """
            ));

        var userDefinedTypes = WildCardImportTypeParser.ParseWildcardTypes(semanticModel);
        var userDefinedType = userDefinedTypes.First(x => x.Name == "exportType");
        Assert.AreEqual("exportType", userDefinedType.Name);
    }

}
