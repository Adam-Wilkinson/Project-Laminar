using Laminar.Contracts.Base.ActionSystem;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Workspaces.PluginLoading;

public interface IPluginManager
{
    public IWorkspace Host { get; }
    
    public IInstalledPluginCollection InstalledPlugins { get; }
    
    public Task<UserActionResult> EnsurePluginInstalled(VersionedPluginId pluginId);
    
    public Task<UserActionResult> UninstallPlugin(string pluginId);
}