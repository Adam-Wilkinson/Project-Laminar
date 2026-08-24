using System.IO.Compression;
using System.Reflection;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Storage.IO;
using Laminar.Domain;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Json;
using Laminar.PluginFramework.Registration;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Base.PluginLoading;

public class PluginInstaller(
    IFileSystem fileSystem, 
    ISharedPluginContext context,
    IPluginHostFactory pluginHostFactory,
    ILogger<PluginInstaller> logger) : IPluginInstaller
{
    public async Task<MayError<IInstalledPlugin>> InstallFromFolder(
        FileSystemPath pluginPath, 
        VersionedPluginId pluginId,
        IRuntimeHost host)
    {
        var manifestPath = pluginPath.ChildPath("manifest.json");
        if (!fileSystem.Exists(manifestPath)) throw new InvalidOperationException("Could not find plugin manifest");
        await using var manifestFile = File.OpenRead(manifestPath);
        var manifest = ManifestData.Parse(manifestFile);
        var entrypointPath = pluginPath.ChildPath((string)manifest.Entrypoint);
        var pluginLoadContext = new PluginLoadContext(entrypointPath, context.DefaultLoadContext);
        var pluginAssembly = pluginLoadContext.LoadFromAssemblyPath(entrypointPath);

        var newPlugin = new InstalledPlugin(pluginHostFactory, host, pluginId);
        foreach (var type in pluginAssembly.GetTypes())
        {
            if (!typeof(IPlugin).IsAssignableFrom(type) || type.IsInterface) continue;

            if (type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, []) is null)
            {
                logger.LogWarning("The type {type} implements IPlugin but has no public parameterless constructor, so cannot be instantiated", type);
                continue;
            }

            if (Activator.CreateInstance(type) is not IPlugin implementation)
            {
                logger.LogWarning("Unknown error creating type {type}", type);
                continue;
            }
            
            newPlugin.AddPluginImplementation(implementation);
        }

        if (newPlugin.ImplementingTypes.Count == 0)
        {
            return new MayError<IInstalledPlugin>(
                new InvalidOperationException($"The plugin at path '{pluginPath}' has no implementation"));
        }

        return new MayError<IInstalledPlugin>(newPlugin);
    }

    public async Task<MayError<IInstalledPlugin>> InstallFromArchive(
        Stream archiveStream, 
        VersionedPluginId pluginId,
        IRuntimeHost runtimeHost)
    {
        var pluginPath = context.OfflineCacheLocation.ChildPath(pluginId.Name).ChildPath(pluginId.Version.ToString());
        await ZipFile.ExtractToDirectoryAsync(archiveStream, pluginPath);
        return await InstallFromFolder(pluginPath, pluginId, runtimeHost);
    }
}