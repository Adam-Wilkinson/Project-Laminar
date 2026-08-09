using System.IO.Compression;
using System.Reflection;
using System.Runtime.Loader;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Storage.IO;
using Laminar.Domain.DataManagement;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Json;
using Laminar.PluginFramework.Registration;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Base.PluginLoading;

internal sealed class PluginInstaller(
    IPluginHostFactory pluginHostFactory,
    IPluginInstallContext pluginInstallContext,
    IPluginRepositoryStore pluginRepositoryStore,
    IFileSystem fileSystem,
    ILogger<IPluginHost> logger)
    : IPluginInstaller
{
    private static readonly FileSystemPath OfflinePluginCache = DataLocations.LocalDataFolder.ChildPath("Plugins");
    
    private readonly Dictionary<(string id, SemanticVersion version), IRegisteredPlugin> _loadedPlugins = [];
    
    public IEnumerable<IRegisteredPlugin> LoadFrom(FileSystemPath pluginPath)
    {
        var pluginName = fileSystem.GetNameWithoutExtension(pluginPath);
        var pluginDllPath = pluginPath.ChildPath(pluginName + ".dll");
        PluginLoadContext pluginContext = new(pluginDllPath, pluginInstallContext.DefaultLoadContext);
        var pluginAssembly = pluginContext.LoadFromAssemblyPath(pluginDllPath);
        foreach (var module in pluginAssembly.Modules)
        {
            foreach (var plugin in GetPluginsFrom(module, pluginInstallContext.FrontendDependency))
            {
                RegisteredPlugin? registeredPlugin = null;
                try
                {
                    registeredPlugin = new RegisteredPlugin(plugin, pluginHostFactory, pluginAssembly);
                    registeredPlugin.Load();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error loading module '{moduleName}'", module.Name);
                }

                if (registeredPlugin is not null)
                {
                    yield return registeredPlugin;
                }
            }
        }
    }

    public async Task<Assembly?> TryGetPluginAssembly(IPluginInfo pluginInfo, SemanticVersion? versionOverride)
    {
        var version = versionOverride ?? pluginInfo.LatestVersion;
        var pluginPath = OfflinePluginCache.ChildPath(pluginInfo.Id).ChildPath(version.ToString());
        if (!fileSystem.Exists(pluginPath))
        {
            if (!pluginInfo.HasVersion(version))
            {
                return null;
            }
            
            fileSystem.CreateDirectory(pluginPath);
            await using var stream = await pluginInfo.OpenVersionStream(version);
            await ZipFile.ExtractToDirectoryAsync(stream, pluginPath);
        }
        
        var manifestPath = pluginPath.ChildPath("manifest.json");
        if (!fileSystem.Exists(manifestPath)) throw new InvalidOperationException("Could not find plugin manifest");
        await using var manifestFile = File.OpenRead(manifestPath);
        var manifest = ManifestData.Parse(manifestFile);
        var entrypointPath = pluginPath.ChildPath((string)manifest.Entrypoint);
        var pluginLoadContext = new PluginLoadContext(entrypointPath, pluginInstallContext.DefaultLoadContext);
        return pluginLoadContext.LoadFromAssemblyPath(entrypointPath);
    }

    public async Task<bool> EnsurePluginInstalled(string pluginId, SemanticVersion version)
    {
        if (_loadedPlugins.ContainsKey((pluginId, version)))
        {
            return true;
        }

        if (await pluginRepositoryStore.GetPluginInfoOrNull(pluginId, version) is not { } pluginInfo)
        {
            return false;
        }

        var loadedAssembly = await TryGetPluginAssembly(pluginInfo, version);
        return loadedAssembly is not null;
    }

    private static IEnumerable<IPlugin> GetPluginsFrom(Module module, FrontendDependency frontendDependency)
    {
        if (module.GetCustomAttribute<HasFrontendDependencyAttribute>() is { } attr && attr.FrontendDependency != frontendDependency)
        {
            yield break;
        }

        foreach (var type in module.GetTypes())
        {
            if (!typeof(IPlugin).IsAssignableFrom(type) || type.IsInterface ||
                Activator.CreateInstance(type) is not IPlugin plugin) continue;
            
            yield return plugin;
        }
    }
}
