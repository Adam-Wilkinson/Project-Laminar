using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.Collections;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.Exceptions;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.Observables.Value;
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
    private readonly ObservableDictionary<VersionedPluginId, IInstalledPlugin> _userInstalledPluginsById = [];
    private readonly ObservableDictionary<VersionedPluginId, IInstalledPlugin> _referencedPluginsById = [];

    public IReadOnlyObservableBag<IInstalledPlugin> UserInstalledPlugins => _userInstalledPluginsById.Values;

    public IReadOnlyObservableBag<IInstalledPlugin> ReferencedPlugins => _referencedPluginsById.Values; 
    
    public Task<IInstalledPlugin?> EnsurePluginInstalled(VersionedPluginId pluginId)
        => EnsurePluginInstalled(pluginId, true);
    
    private async Task<IInstalledPlugin?> EnsurePluginInstalled(VersionedPluginId pluginId, bool userInstalled)
    {
        if (_userInstalledPluginsById.TryGetValue(pluginId, out var plugin))
        {
            return plugin;
        }

        if (_referencedPluginsById.TryGetValue(pluginId, out var referencedPlugin))
        {
            if (userInstalled)
            {
                _referencedPluginsById.Remove(pluginId);
                _userInstalledPluginsById.Remove(pluginId);
            }
            
            return referencedPlugin;
        }

        if (await library.GetPluginSourceOrNull(pluginId) is not { } pluginSource)
        {
            return null;
        }

        var manifest = await pluginSource.GetManifest(pluginId);
        List<IInstalledPlugin> installedDependencies = [];
        foreach (var (dependencyId, frontend) in manifest.GetDependencies())
        {
            if (frontend is not FrontendDependency.None && context.FrontendDependency != frontend)
            {
                LogSkippingDependency(dependencyId, frontend);
                continue;
            }

            if (await EnsurePluginInstalled(dependencyId, false) is not { } installedDependency)
            {
                while (installedDependencies.Count > 0)
                {
                    UninstallPlugin(installedDependencies.First().PluginId);
                }
                
                await exceptionHandler.OnExceptionAsync(new DependentPluginNotFoundException(pluginId, dependencyId));
                return null;
            }
            
            installedDependencies.Add(installedDependency);
        }
        
        if (!(await pluginSource.InstallPlugin(pluginId, host)).TryGetResult(out var newPlugin, out var ex))
        {
            await exceptionHandler.OnExceptionAsync(ex);
            return null;
        }
        
        _userInstalledPluginsById.Add(pluginId, newPlugin);
        context.RegisterInstallation(newPlugin);
        return newPlugin;
    }

    public void UninstallPlugin(VersionedPluginId pluginId)
    {
        throw new NotImplementedException();
    }

    public bool TryGetInstalledPlugin(VersionedPluginId pluginId, [NotNullWhen(true)] out IInstalledPlugin? plugin)
        => _userInstalledPluginsById.TryGetValue(pluginId, out plugin);

    [LoggerMessage(LogLevel.Trace, "Skipping dependency {dependencyId} since its frontend dependency {frontend} is not present")]
    partial void LogSkippingDependency(VersionedPluginId dependencyId, FrontendDependency frontend);
}