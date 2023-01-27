using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.PluginFramework.Registration;

namespace Laminar.Implementation.Base.PluginLoading;

internal class PluginHostFactory : IPluginHostFactory
{
    private readonly ITypeInfoStore _typeInfoStore;
    private readonly ILoadedNodeManager _loadedNodeManager;
    private readonly IUserInterfaceStore _userInterfaceStore;
    private readonly IConnectorFactory _connectorViewFactory;

    public PluginHostFactory(
        ITypeInfoStore typeInfoStore,
        ILoadedNodeManager loadedNodeManager,
        IUserInterfaceStore userInterfaceStore,
        IConnectorFactory connectorViewFactory)
    {
        _typeInfoStore = typeInfoStore;
        _loadedNodeManager = loadedNodeManager;
        _userInterfaceStore = userInterfaceStore;
        _connectorViewFactory = connectorViewFactory;
    }

    public IPluginHost GetPluginhost(IRegisteredPlugin registeredPlugin)
    {
        return new PluginHost(registeredPlugin, _typeInfoStore, _loadedNodeManager, _userInterfaceStore, _connectorViewFactory);
    }
}
