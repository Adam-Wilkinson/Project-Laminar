using Laminar.Contracts.Base.ActionSystem;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Base.ActionSystem;

namespace Laminar.Implementation.Workspaces.PluginLoading.Actions;

internal readonly struct InstallPluginAction(VersionedPluginId pluginId, PluginActionContext context) : IUserAction
{
    public async Task<IUserActionExecutionOutcome> Execute()
    {
        var currentVersion = context.Workspace.PluginManager.GetPluginVersion(pluginId.Name);
        if (currentVersion is { } installedPluginVersion && installedPluginVersion == pluginId.Version)
        {
            return IUserActionExecutionOutcome.Ineffectual();
        }

        if (await context.PluginLibrary.GetPluginSourceOrNull(pluginId) is not { } pluginSource)
        {
            return IUserActionExecutionOutcome.Error(new Exception($"Plugin library could not find plugin {pluginId}"));
        }

        if (currentVersion is not null)
        {
            return IUserActionExecutionOutcome.Alternative(new CompoundAction(new UninstallPluginAction(), this));
        }
        
        if (!(await pluginSource.InstallPlugin(pluginId, context.Workspace)).TryGetResult(out var newPlugin, out var ex))
        {
            return IUserActionExecutionOutcome.Error(ex);
        }
        
        context.UserInstalledPluginsById[pluginId.Name] = newPlugin;
        return IUserActionExecutionOutcome.Success(new UninstallPluginAction(pluginId.Name, context));
    }
}