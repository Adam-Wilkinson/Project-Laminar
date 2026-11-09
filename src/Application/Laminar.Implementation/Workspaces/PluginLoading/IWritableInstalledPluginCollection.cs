using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Workspaces.PluginLoading;
    
namespace Laminar.Implementation.Workspaces.PluginLoading;

public interface IWritableInstalledPluginCollection : IInstalledPluginCollection
{
    public void Add(string pluginId, IPluginInstallation installation);
    
    public bool TryGetInstallation(string pluginId, [MaybeNullWhen(false)] out IPluginInstallation installation);

    public void Remove(string pluginId);
    
    public bool Remove(string pluginId, [MaybeNullWhen(false)] out IPluginInstallation installation);

    public IDisposable CreateModificationScope();
}