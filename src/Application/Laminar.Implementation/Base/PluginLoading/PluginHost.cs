using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain;
using Laminar.Implementation.Scripting.NodeWrapping;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.Registration;
using Laminar.PluginFramework.Serialization;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Base.PluginLoading;

internal sealed class PluginHost(
    InstalledPlugin plugin,
    ILoadedNodeManager loadedNodeManager,
    ITypeInfoStore typeInfoStore,
    IDataInterfaceFactory dataInterfaceFactory,
    ISerializer serializer)
    : IReleasablePluginHost
{
    public void AddNodeToMenu<TNode>(string menuItemName, string? subItemName = null) where TNode : INode, new()
    {
        LoadedNodeInfo<TNode> newNodeInfo = new(plugin);
        plugin.AddNode(newNodeInfo.NodeDescriptor.NodeName, newNodeInfo);
        loadedNodeManager.AddNodeToCategory(
            subItemName is null
                ? menuItemName
                : $"{menuItemName}{ItemCategory<INodeContainer>.SeparationChar}{subItemName}", 
            newNodeInfo);
    }

    public bool RegisterDataInterfaceFactory<TInterfaceDefinition, TData, TInterface>(Func<TInterface> factory)
        where TInterfaceDefinition : IUserInterfaceDefinition, new()
        where TData : notnull
        where TInterface : class
    {
        dataInterfaceFactory.RegisterInterfaceFactory<TInterfaceDefinition, TData, TInterface>(factory);
        return true;
    }

    public bool RegisterType<T>(string hexColour, string userFriendlyName, T defaultValue, IUserInterfaceDefinition defaultEditor, IUserInterfaceDefinition defaultDisplay, TypeSerializer<T>? typeSerializer)
        where T : notnull
    {
        if (typeSerializer is not null)
        {
            serializer.RegisterSerializer(typeSerializer);
        }

        typeInfoStore.RegisterType(typeof(T), new TypeInfo(userFriendlyName, defaultEditor, defaultDisplay, hexColour, defaultValue!));
        return true;
    }

    public bool TryAddTypeConverter<TInput, TOutput, TConverter>() where TConverter : INode
        => throw new NotImplementedException();
    
    public void UnregisterAll()
    {
        throw new NotImplementedException();
    }
}
