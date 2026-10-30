using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Workspaces.PluginLoading;

public interface IPluginHostFactory
{
    public IPluginInstallation GetPluginHost(VersionedPluginId pluginId, ILoadedNodeManager loadedNodeManager);

    public IDisposable CreatePluginRegistrationScope();
}
