using Azure.Deployments.Core.Definitions.Schema;
using Bicep.Core.Semantics;
using LandingZones.Tools.BicepDocs.Core.Parsers;

namespace LandingZones.Tools.BicepDocs.Core.UnitTests.Parsers;

[TestClass]
public class ImportTypeParserTests : BicepFileTestBase
{

    [TestMethod]
    public async Task Test_ImportedTypesExplicit()
    {
        var semanticModel = await GetModel(
            ("main.bicep", """
                           import {exportType} from 'import.bicep'
                           """),
            ("import.bicep", """
                          @export()
                          @description('This is an exportType')
                          type exportType = {
                            @description('This is a boolean type')
                            booleanProperty: bool
                            stringProperty: string?
                            literalType: 'dev' | 'prd'
                            @minValue(1)
                            @maxValue(10)
                            intType: int
                            @minLength(1)
                            @maxLength(5)
                            stringConstraint: string?
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

        var secureStringProperty = userDefinedType.Properties.First(x => x.Name == "secureString");
        Assert.IsTrue(secureStringProperty.Secure);

        var stringConstraintProperty = userDefinedType.Properties.First(x => x.Name == "stringConstraint");
        Assert.IsFalse(stringConstraintProperty.IsRequired);
        Assert.AreEqual(1, stringConstraintProperty.MinLength);
        Assert.AreEqual(5, stringConstraintProperty.MaxLength);
        Assert.AreEqual("string", stringConstraintProperty.Type);

        var literalTypeProperty = userDefinedType.Properties.First(x => x.Name == "literalType");
        Assert.AreEqual(2, literalTypeProperty.AllowedValues.Count);

        var booleanProperty = userDefinedType.Properties.First(x => x.Name == "booleanProperty");
        Assert.AreEqual("bool", booleanProperty.Type);

    }

    [TestMethod]
    public async Task Test_ValidtionsOnType()
    {
        var semanticModel = await GetModel(
            ("main.bicep", """
                           import {exportType} from 'import.bicep'
                           
                           param test exportType
                           """),
            ("import.bicep", """
                
                          @description('This is an exportType')
                          @export()
                          @minLength(0)
                          @maxLength(18)
                          type exportType = string
                          """
        ));

        var userDefinedTypes = ImportTypeParser.ParseImportTypes(semanticModel);
        var userDefinedType = userDefinedTypes.First(x => x.Name == "exportType");
        Assert.AreEqual("exportType", userDefinedType.Name);
    }

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

        var userDefinedTypes = ImportTypeParser.ParseImportTypes(semanticModel);
        var userDefinedType = userDefinedTypes.First(x => x.Name == "exportType");
        Assert.AreEqual("exportType", userDefinedType.Name);
    }

}
