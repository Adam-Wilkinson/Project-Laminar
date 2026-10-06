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
    public async Task<MayError<IPluginInstallation>> InstallFromFolder(
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

        using var _ = pluginHostFactory.CreatePluginRegistrationScope();
        var pluginHost = pluginHostFactory.GetPluginHost(pluginId, host.NodeManager);
        var implementations = 0;
        foreach (var type in pluginAssembly.GetTypes())
        {
            if (!typeof(IPlugin).IsAssignableFrom(type) || type.IsInterface) continue;

            if (type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, []) is null)
            {
                logger.LogWarning("The type {Type} implements IPlugin but has no public parameterless constructor, so cannot be instantiated", type);
                continue;
            }

            if (Activator.CreateInstance(type) is not IPlugin implementation)
            {
                logger.LogWarning("Unknown error creating type {Type}", type);
                continue;
            }

            try
            {
                implementation.Register(pluginHost);
                implementations++;
            }
            catch (Exception ex)
            {
                return new MayError<IPluginInstallation>(ex);
            }
        }
        
        if (implementations == 0)
        {
            return new MayError<IPluginInstallation>(
                new InvalidOperationException($"The plugin at path '{pluginPath}' has no implementation"));
        }

        return new MayError<IPluginInstallation>(pluginHost);
    }

    public async Task<MayError<IPluginInstallation>> InstallFromArchive(
        Stream archiveStream, 
        VersionedPluginId pluginId,
        IRuntimeHost runtimeHost)
    {
        var pluginPath = context.OfflineCacheLocation.ChildPath(pluginId.Name).ChildPath(pluginId.Version.ToString());
        await ZipFile.ExtractToDirectoryAsync(archiveStream, pluginPath);
        return await InstallFromFolder(pluginPath, pluginId, runtimeHost);
    }
}