using System.Collections.ObjectModel;
using System.IO.Abstractions;
using System.Runtime.InteropServices.ComTypes;
using Bicep.Core;
using Bicep.Core.FileSystem;
using Bicep.Core.Semantics;
using Bicep.Core.Workspaces;
using LandingZones.Tools.BicepDocs.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace LandingZones.Tools.BicepDocs.Core.UnitTests;

public abstract class BicepFileTestBase
{
    protected readonly IFileSystem FileSystem;
    protected readonly IServiceProvider ServiceProvider;

    protected BicepFileTestBase()
    {

        var sp = new ServiceCollection();
        sp.AddBicepCore().AddBicepFileService();
        ServiceProvider = new DefaultServiceProviderFactory().CreateBuilder(sp).BuildServiceProvider();
        FileSystem = ServiceProvider.GetRequiredService<IFileSystem>();
    }

    protected async Task<SemanticModel> GetModelFromPath(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var fileContent = await File.ReadAllTextAsync(filePath);
        return await GetModel(fileContent, fileName);
    }

    protected async Task<SemanticModel> GetModel(string fileContent, string? importContent = null, string fileName = "deploy.bicep", string importFileName = "import.bicep")
    {
        var vPath = Path.Join("/modules", fileName).ToPlatformPath();
        FileSystem.Directory.CreateDirectory("/modules".ToPlatformPath());
        await FileSystem.File.WriteAllTextAsync(vPath, fileContent);

        if (!string.IsNullOrEmpty(importContent))
        {
            var importPath = Path.Join("/modules", importFileName).ToPlatformPath();
            await FileSystem.File.WriteAllTextAsync(importPath, importContent);
        }

        var compiler = ServiceProvider.GetRequiredService<BicepCompiler>();
        var compilation = await compiler.CreateCompilation(PathResolver.FilePathToUri(vPath));
        return compilation.GetEntrypointSemanticModel();
    }


    protected async Task<SemanticModel> RestoreAndCompile(string fileContent, string importContent,
        string fileName = "deploy.bicep", string importFileName = "import.bicep")
    {

        var vPath = Path.Join("/modules", fileName).ToPlatformPath();
        FileSystem.Directory.CreateDirectory("/modules".ToPlatformPath());
        await FileSystem.File.WriteAllTextAsync(vPath, fileContent);

        //if (!string.IsNullOrEmpty(importContent))
        //{
        var importPath = Path.Join("/modules", importFileName).ToPlatformPath();
        await FileSystem.File.WriteAllTextAsync(importPath, importContent);
        //}

        var files = new Dictionary<Uri, string>()
        {
            { PathResolver.FilePathToUri(vPath), fileContent },
            { PathResolver.FilePathToUri(importPath), importContent }
        };
        var workspace = CreateWorkspace(files);

        var compiler = ServiceProvider.GetRequiredService<BicepCompiler>();
        var compilation = await compiler.CreateCompilation(PathResolver.FilePathToUri(vPath), workspace);
        return compilation.GetEntrypointSemanticModel();
    }

    public static IWorkspace CreateWorkspace(IReadOnlyDictionary<Uri, string> uriDictionary)
    {
        var workspace = new Workspace();
        var sourceFiles = uriDictionary.Select(kvp => SourceFileFactory.CreateSourceFile(kvp.Key, kvp.Value));
        workspace.UpsertSourceFiles(sourceFiles);

        return workspace;
    }
}
