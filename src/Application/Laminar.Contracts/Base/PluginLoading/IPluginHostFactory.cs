using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.PluginFramework.Registration;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginHostFactory
{
    public IPluginHost GetPluginHost(IInstalledPlugin plugin, ILoadedNodeManager loadedNodeManager);
}
