using Laminar.Contracts.NodeSystem;
using Laminar.Contracts.NodeSystem.Connection;
using Laminar.Contracts.UserInterface;

namespace Laminar.Core.PluginManagement;

internal class PluginHostFactory : IPluginHostFactory
{
    private readonly ITypeInfoStore _typeInfoStore;
    private readonly ILoadedNodeManager _loadedNodeManager;
    private readonly IUserInterfaceStore _userInterfaceStore;
    private readonly IConnectorViewFactory _connectorViewFactory;

    public PluginHostFactory(
        ITypeInfoStore typeInfoStore,
        ILoadedNodeManager loadedNodeManager,
        IUserInterfaceStore userInterfaceStore,
        IConnectorViewFactory connectorViewFactory)
    {
        _typeInfoStore = typeInfoStore;
        _loadedNodeManager = loadedNodeManager;
        _userInterfaceStore = userInterfaceStore;
        _connectorViewFactory = connectorViewFactory;
    }

    public PluginHost GetPluginhost(IRegisteredPlugin registeredPlugin)
    {
        return new PluginHost(registeredPlugin, _typeInfoStore, _loadedNodeManager, _userInterfaceStore, _connectorViewFactory);
    }
}
