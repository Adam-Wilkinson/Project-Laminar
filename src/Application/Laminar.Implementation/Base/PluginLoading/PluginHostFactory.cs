using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.PluginFramework.Registration;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Base.PluginLoading;

internal class PluginHostFactory(
    ITypeInfoStore typeInfoStore,
    ILoadedNodeManager loadedNodeManager,
    IDataInterfaceFactory dataInterfaceFactory,
    ISerializer serializer)
    : IPluginHostFactory
{
    public IPluginHost GetPluginHost(IRegisteredPlugin registeredPlugin)
    {
        return new PluginHost(registeredPlugin, typeInfoStore, loadedNodeManager, dataInterfaceFactory, serializer);
    }
}
