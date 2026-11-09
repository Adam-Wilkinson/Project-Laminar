using Laminar.Contracts.Base;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Workspaces;
using Laminar.Contracts.Workspaces.PluginLoading;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Workspaces.PluginLoading.Actions;

namespace Laminar.Implementation.Workspaces.PluginLoading;

internal sealed class PluginManager(IWorkspace host, IPluginLibrary library, IExceptionHandler exceptionHandler) 
    : IPluginManager
{
    private readonly IWritableInstalledPluginCollection _userInstalledPlugins = new WritableInstalledPluginCollection(host);

    private PluginActionContext Context => field ??= new PluginActionContext(host, library, _userInstalledPlugins);

    public IInstalledPluginCollection InstalledPlugins => _userInstalledPlugins;
    
    public IWorkspace Host => host;

    public async Task<UserActionResult> EnsurePluginInstalled(VersionedPluginId pluginId)
    {
        var result = await Host.ActionScope.ExecuteAction(new InstallPluginAction(pluginId, Context));
        if (!result.Succeeded)
        {
            await exceptionHandler.OnExceptionsAsync(result.Exceptions);
        }
        
        return result;
    }

    public async Task<UserActionResult> UninstallPlugin(string pluginId)
    {
        var result = await Host.ActionScope.ExecuteAction(new UninstallPluginAction(pluginId, Context));
        if (!result.Succeeded)
        {
            await exceptionHandler.OnExceptionsAsync(result.Exceptions);
        }

        return result;
    }
}