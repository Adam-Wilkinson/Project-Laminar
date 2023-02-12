using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.PluginFramework.Registration;

namespace Laminar.Implementation.Base.PluginLoading;

internal class PluginHostFactory : IPluginHostFactory
{
    private readonly ITypeInfoStore _typeInfoStore;
    private readonly ILoadedNodeManager _loadedNodeManager;
    private readonly IUserInterfaceStore _userInterfaceStore;

    public PluginHostFactory(
        ITypeInfoStore typeInfoStore,
        ILoadedNodeManager loadedNodeManager,
        IUserInterfaceStore userInterfaceStore)
    {
        _typeInfoStore = typeInfoStore;
        _loadedNodeManager = loadedNodeManager;
        _userInterfaceStore = userInterfaceStore;
    }

    public IPluginHost GetPluginhost(IRegisteredPlugin registeredPlugin)
    {
        return new PluginHost(registeredPlugin, _typeInfoStore, _loadedNodeManager, _userInterfaceStore);
    }
}
