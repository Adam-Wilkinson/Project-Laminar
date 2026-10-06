using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Scripting.NodeWrapping;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.Registration;
using Laminar.PluginFramework.Serialization;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Base.PluginLoading;

internal sealed class PluginHost(
    VersionedPluginId pluginId,
    ILoadedNodeManager loadedNodeManager,
    ITypeInfoStore typeInfoStore,
    IDataInterfaceFactory dataInterfaceFactory,
    ISerializer serializer)
    : IPluginHost, IPluginInstallation
{
    private readonly List<(ILoadedNodeInfo nodeInfo, string categoryPath)> _installedNodes = [];
    private readonly List<Type> _loadedTypeInfos = [];
    private readonly CompositeDisposable _disposables = new();

    public VersionedPluginId PluginId => pluginId;
    
    public void AddNodeToMenu<TNode>(string menuItemName, string? subItemName = null) where TNode : INode, new()
    {
        LoadedNodeInfo<TNode> newNodeInfo = new(pluginId);
        var path = subItemName is null
            ? menuItemName
            : $"{menuItemName}{ItemCategory<INodeContainer>.SeparationChar}{subItemName}"; 
        loadedNodeManager.AddNodeToCategory(path, newNodeInfo);
        _installedNodes.Add((newNodeInfo, path));
    }

    public bool RegisterDataInterfaceFactory<TInterfaceDefinition, TData, TInterface>(Func<TInterface> factory)
        where TInterfaceDefinition : IUserInterfaceDefinition, new()
        where TData : notnull
        where TInterface : class
    {
        _disposables.Add(dataInterfaceFactory
            .RegisterInterfaceFactory<TInterfaceDefinition, TData, TInterface>(factory));
        return true;
    }

    public bool RegisterType<T>(string hexColour, string userFriendlyName, T defaultValue, IUserInterfaceDefinition defaultEditor, IUserInterfaceDefinition defaultDisplay, TypeSerializer<T>? typeSerializer)
        where T : notnull
    {
        if (typeSerializer is not null)
        {
            serializer.RegisterSerializer(typeSerializer);
        }

        typeInfoStore.RegisterType(typeof(T), new TypeInfo(userFriendlyName, defaultEditor, defaultDisplay, hexColour, defaultValue));
        _loadedTypeInfos.Add(typeof(T));
        return true;
    }

    public bool TryAddTypeConverter<TInput, TOutput, TConverter>() where TConverter : INode => throw new NotImplementedException();
    
    public void Uninstall()
    {
        foreach (var (node, path) in _installedNodes)
        {
            loadedNodeManager.RemoveNodeFromCategory(path, node);
        }

        foreach (var type in _loadedTypeInfos)
        {
            typeInfoStore.UnregisterType(type);
        }
        
        _disposables.Dispose();
    }
}
