// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.IO.Abstractions;
using Bicep.Core;
using Bicep.Core.Analyzers.Interfaces;
using Bicep.Core.Analyzers.Linter;
using Bicep.Core.Configuration;
using Bicep.Core.Features;
using Bicep.Core.FileSystem;
using Bicep.Core.Registry;
using Bicep.Core.Registry.Auth;
using Bicep.Core.Semantics.Namespaces;
using Bicep.Core.TypeSystem.Providers;
using Bicep.Core.TypeSystem.Providers.Az;
using Bicep.Core.TypeSystem.Types;
using Bicep.Core.Utils;
using Bicep.Core.Workspaces;
using Bicep.Decompiler;
using Microsoft.Extensions.DependencyInjection;
using IOFileSystem = System.IO.Abstractions.FileSystem;

namespace LandingZones.Tools.BicepDocs.Core.UnitTests;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddBicepCore(this IServiceCollection services) => services
        .AddSingleton<INamespaceProvider, DefaultNamespaceProvider>()
        .AddSingleton<IResourceTypeProviderFactory, ResourceTypeProviderFactory>()
        .AddSingleton<IContainerRegistryClientFactory, ContainerRegistryClientFactory>()
        .AddSingleton<ITemplateSpecRepositoryFactory, TemplateSpecRepositoryFactory>()
        .AddSingleton<IModuleDispatcher, ModuleDispatcher>()
        .AddSingleton<IArtifactRegistryProvider, DefaultArtifactRegistryProvider>()
        .AddSingleton<ITokenCredentialFactory, TokenCredentialFactory>()
        .AddSingleton<IFileResolver, FileResolver>()
        .AddSingleton<IEnvironment, Bicep.Core.Utils.Environment>()
        .AddSingleton<IFileSystem, IOFileSystem>()
        .AddSingleton<IConfigurationManager, ConfigurationManager>()
        .AddSingleton<IBicepAnalyzer, LinterAnalyzer>()
        .AddSingleton<IFeatureProviderFactory, FeatureProviderFactory>()
        .AddSingleton<ILinterRulesProvider, LinterRulesProvider>()
        .AddSingleton<BicepCompiler>();

    public static IServiceCollection AddBicepDecompiler(this IServiceCollection services) => services
        .AddSingleton<BicepDecompiler>();

    private static IServiceCollection Register<TService>(IServiceCollection services, TService service)
        where TService : class
        => services.AddSingleton(service);

    public static IServiceCollection WithFileResolver(this IServiceCollection services, IFileResolver fileResolver)
        => Register(services, fileResolver);

    public static IServiceCollection WithFileSystem(this IServiceCollection services, IFileSystem fileSystem)
        => Register(services, fileSystem);

    public static IServiceCollection WithContainerRegistryClientFactory(this IServiceCollection services, IContainerRegistryClientFactory containerRegistryClientFactory)
        => Register(services, containerRegistryClientFactory);

    public static IServiceCollection WithWorkspace(this IServiceCollection services, IWorkspace workspace)
        => Register(services, workspace);

    public static IServiceCollection WithAzResources(this IServiceCollection services, IEnumerable<ResourceTypeComponents> resourceTypes)
        => services.WithAzResourceTypeLoaderFactory(TestTypeHelper.CreateResourceTypeLoaderWithTypes(resourceTypes));

    public static IServiceCollection WithAzResourceTypeLoaderFactory(this IServiceCollection services, IResourceTypeLoader loader)
    {
        var provider = new AzResourceTypeProvider(loader, AzNamespaceType.Settings.ArmTemplateProviderVersion);
        return Register(services, TestTypeHelper.CreateResourceTypeLoaderFactory(provider));
    }
}
