// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using Bicep.Core.FileSystem;
using Bicep.Core.Semantics;
using Bicep.Core.TypeSystem.Providers;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace LandingZones.Tools.BicepDocs.Core.UnitTests;

public static class ServiceBuilderExtensions
{
    public static ServiceBuilder WithFileResolver(this ServiceBuilder serviceBuilder, IFileResolver fileResolver)
        => serviceBuilder.WithRegistration(x => x.WithFileResolver(fileResolver));

    public static ServiceBuilder WithAzResourceTypeLoader(this ServiceBuilder serviceBuilder, IResourceTypeLoader azResourceTypeLoader)
        => serviceBuilder.WithRegistration(x => x.WithAzResourceTypeLoaderFactory(azResourceTypeLoader));

    public static ServiceBuilder WithFileSystem(this ServiceBuilder serviceBuilder, IFileSystem fileSystem)
        => serviceBuilder.WithRegistration(x => x.WithFileSystem(fileSystem));

    public static ServiceBuilder WithMockFileSystem(this ServiceBuilder serviceBuilder, IReadOnlyDictionary<Uri, string>? fileLookup = null)
        => serviceBuilder.WithFileSystem(new MockFileSystem(
            fileLookup?.ToDictionary(x => x.Key.LocalPath, x => new MockFileData(x.Value)) ?? new()));

    public static Compilation BuildCompilation(this ServiceBuilder services, IReadOnlyDictionary<Uri, string> fileContentsByUri, Uri entryFileUri)
    {
        var compiler = services.Build().GetCompiler();
        var workspace = CompilationHelper.CreateWorkspace(fileContentsByUri);

        return compiler.CreateCompilationWithoutRestore(entryFileUri, workspace);
    }

}
