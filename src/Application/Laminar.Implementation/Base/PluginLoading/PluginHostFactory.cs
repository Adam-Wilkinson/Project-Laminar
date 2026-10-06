using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Implementation.Base.PluginLoading;

internal sealed class PluginHostFactory(
    ITypeInfoStore typeInfoStore,
    IDataInterfaceFactory dataInterfaceFactory,
    ISerializer serializer)
    : IPluginHostFactory
{
    public IPluginInstallation GetPluginHost(VersionedPluginId pluginId, ILoadedNodeManager loadedNodeManager)
    {
        return new PluginHost(pluginId, loadedNodeManager, typeInfoStore, dataInterfaceFactory, serializer);
    }

    public IDisposable CreatePluginRegistrationScope() => dataInterfaceFactory.CreateRegistrationScope();
}
