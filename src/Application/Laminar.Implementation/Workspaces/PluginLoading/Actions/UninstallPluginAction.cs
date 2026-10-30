using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Implementation.Workspaces.PluginLoading.Actions;

internal readonly struct UninstallPluginAction(string pluginId, PluginActionContext context) : IUserAction
{
    public Task<IUserActionExecutionOutcome> Execute()
    {
        if (!context.UserInstalledPluginsById.Remove(pluginId, out var plugin))
        {
            return Task.FromResult(IUserActionExecutionOutcome.Ineffectual());
        }
        
        plugin.Uninstall();
        return Task.FromResult(IUserActionExecutionOutcome.Success(new InstallPluginAction(plugin.PluginId, context)));
    }
}