using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.PluginFramework.Registration;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Base.PluginLoading;

internal sealed class PluginHostFactory(
    ITypeInfoStore typeInfoStore,
    IDataInterfaceFactory dataInterfaceFactory,
    ISerializer serializer)
    : IPluginHostFactory
{
    public IPluginHost GetPluginHost(IInstalledPlugin plugin, ILoadedNodeManager loadedNodeManager)
    {
        return new PluginHost((InstalledPlugin)plugin, loadedNodeManager, typeInfoStore, dataInterfaceFactory, serializer);
    }
}
