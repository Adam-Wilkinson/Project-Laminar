using System.Diagnostics.CodeAnalysis;
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
    private readonly ObservableList<VersionedPluginId> _userInstalledPluginsById = [];

    public IReadOnlyObservableBag<VersionedPluginId> UserInstalledPlugins => _userInstalledPluginsById;

    public IRuntimeHost Host => host;

    public Task<bool> EnsurePluginInstalled(VersionedPluginId pluginId)
        => EnsurePluginInstalled(pluginId, true);

    public bool PluginInstalled(VersionedPluginId pluginId) => _userInstalledPluginsById.Contains(pluginId);

    private async Task<bool> EnsurePluginInstalled(VersionedPluginId pluginId, bool userInstalled)
    {
        if (_userInstalledPluginsById.Contains(pluginId))
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

            if (!await EnsurePluginInstalled(dependencyId, false))
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
        
        _userInstalledPluginsById.Add(pluginId);
        return newPlugin;
    }

    public void UninstallPlugin(VersionedPluginId pluginId)
    {
        throw new NotImplementedException();
    }

    [LoggerMessage(LogLevel.Trace, "Skipping dependency {DependencyId} since its frontend dependency {Frontend} is not present")]
    partial void LogSkippingDependency(VersionedPluginId dependencyId, FrontendDependency frontend);
}