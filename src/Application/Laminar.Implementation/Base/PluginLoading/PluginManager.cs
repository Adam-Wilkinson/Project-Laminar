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
    private readonly ObservableDictionary<VersionedPluginId, IPluginInstallation> _userInstalledPluginsById = [];

    public IReadOnlyObservableBag<VersionedPluginId> UserInstalledPlugins => _userInstalledPluginsById.Keys;

    public IRuntimeHost Host => host;

    public bool PluginInstalled(VersionedPluginId pluginId) => _userInstalledPluginsById.ContainsKey(pluginId);
    
    public async Task<bool> EnsurePluginInstalled(VersionedPluginId pluginId)
    {
        if (PluginInstalled(pluginId))
        {
            return true;
        }

        if (await library.GetPluginSourceOrNull(pluginId) is not { } pluginSource)
        {
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
                    UninstallPlugin(installedDependencies.First());
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
        
        _userInstalledPluginsById.Add(pluginId, newPlugin);
        return true;
    }

    public void UninstallPlugin(VersionedPluginId pluginId)
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