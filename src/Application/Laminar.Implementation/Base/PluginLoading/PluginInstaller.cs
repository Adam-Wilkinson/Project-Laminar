using System.IO.Compression;
using System.Reflection;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Storage.IO;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Json;
using Laminar.PluginFramework.Registration;

namespace Laminar.Implementation.Base.PluginLoading;

public class PluginInstaller(
    IFileSystem fileSystem, 
    ISharedPluginContext context,
    IPluginHostFactory pluginHostFactory) : IPluginInstaller
{
    public async Task<IInstalledPlugin> InstallFromFolder(
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
            if (typeof(IPlugin).IsAssignableFrom(type) && !type.IsInterface &&
                type.GetConstructor(BindingFlags.Public, []) is not null
                && Activator.CreateInstance(type) is IPlugin pluginFront)
            {
                newPlugin.AddPluginImplementation(pluginFront);
            }
        }

        return newPlugin;
    }

    public async Task<IInstalledPlugin> InstallFromArchive(
        Stream archiveStream, 
        VersionedPluginId pluginId,
        IRuntimeHost runtimeHost)
    {
        var pluginPath = context.OfflineCacheLocation.ChildPath(pluginId.Name).ChildPath(pluginId.Version.ToString());
        await ZipFile.ExtractToDirectoryAsync(archiveStream, pluginPath);
        return await InstallFromFolder(pluginPath, pluginId, runtimeHost);
    }
}