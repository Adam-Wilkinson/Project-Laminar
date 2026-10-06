using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.Exceptions;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Extensions;
using Laminar.PluginFramework.Registration;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Base.PluginLoading;

internal sealed partial class PluginManager(
    IRuntimeHost host, 
    ISharedPluginContext context,
    IPluginLibrary library,
    IExceptionHandler exceptionHandler,
    ILogger<PluginManager> logger) 
    : IPluginManager
{
    private readonly ObservableDictionary<string, IPluginInstallation> _userInstalledPluginsById = [];
    
    public IReadOnlyObservableBag<string> UserInstalledPlugins => _userInstalledPluginsById.Keys;

    public IRuntimeHost Host => host;

    public SemanticVersion? GetInstalledPluginVersion(string pluginId) 
        => _userInstalledPluginsById.TryGetValue(pluginId, out var plugin) ? plugin.PluginId.Version : null;
    
    public async Task<bool> EnsurePluginInstalled(VersionedPluginId pluginId)
    {
        if (GetInstalledPluginVersion(pluginId.Name) is { } installedPluginVersion)
        {
            if (installedPluginVersion == pluginId.Version) return true;
            
            await exceptionHandler.OnExceptionAsync(new InvalidOperationException($"Cannot install plugin {pluginId.Name} version {pluginId.Version} because version {installedPluginVersion} is already installed"));
            return false;

        }

        if (await library.GetPluginSourceOrNull(pluginId) is not { } pluginSource)
        {
            logger.LogError("Plugin library could not find plugin {PluginId}", pluginId);
            return false;
        }

        var manifest = await pluginSource.GetManifest(pluginId);
        List<VersionedPluginId> installedDependencies = [];
        foreach (var (dependencyId, frontend) in manifest.GetDependencies())
        {
            if (frontend is not FrontendDependency.None && context.FrontendDependency != frontend)
            {
                LogSkippingDependency(dependencyId, frontend);
                continue;
            }

            if (!await EnsurePluginInstalled(dependencyId))
            {
                while (installedDependencies.Count > 0)
                {
                    UninstallPlugin(installedDependencies.First().Name);
                }
                
                await exceptionHandler.OnExceptionAsync(new DependentPluginNotFoundException(pluginId, dependencyId));
                return false;
            }
            
            installedDependencies.Add(dependencyId);
        }
        
        if (!(await pluginSource.InstallPlugin(pluginId, Host)).TryGetResult(out var newPlugin, out var ex))
        {
            await exceptionHandler.OnExceptionAsync(ex);
            return false;
        }
        
        _userInstalledPluginsById.Add(pluginId.Name, newPlugin);
        return true;
    }

    public void UninstallPlugin(string pluginId)
    {
        if (!_userInstalledPluginsById.TryGetValue(pluginId, out var plugin))
        {
            return;
        }
        
        plugin.Uninstall();
        _userInstalledPluginsById.Remove(pluginId);
    }

    [LoggerMessage(LogLevel.Trace, "Skipping dependency {DependencyId} since its frontend dependency {Frontend} is not present")]
    partial void LogSkippingDependency(VersionedPluginId dependencyId, FrontendDependency frontend);
}