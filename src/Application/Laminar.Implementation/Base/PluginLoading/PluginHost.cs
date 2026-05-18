using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.Registration;
using Laminar.PluginFramework.Serialization;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Base.PluginLoading;

public class PluginHost(
    IRegisteredPlugin registeredPlugin,
    ITypeInfoStore typeInfoStore,
    ILoadedNodeManager loadedNodeManager,
    IUserInterfaceStore userInterfaceStore,
    IDataInterfaceFactory dataInterfaceFactory,
    ISerializer serializer)
    : IPluginHost
{
    public void AddNodeToMenu<TNode>(string menuItemName, string? subItemName = null) where TNode : INode, new()
    {
        TNode node = new();
        registeredPlugin.RegisterNode(node);
        loadedNodeManager.AddNodeToCategory(node, subItemName is null ? menuItemName : $"{menuItemName}{ItemCategory<IWrappedNode>.SeparationChar}{subItemName}");
    }

    public bool RegisterDataInterfaceFactory<TInterfaceDefinition, TData, TInterface>(Func<TInterface> factory)
        where TInterfaceDefinition : IUserInterfaceDefinition, new()
        where TData : notnull
        where TInterface : class
    {
        dataInterfaceFactory.RegisterInterfaceFactory<TInterfaceDefinition, TData, TInterface>(factory);
        return true;
    }
    
    public bool RegisterDataInterface<TInterfaceDefinition, TData, TInterface>() 
        where TInterfaceDefinition : IUserInterfaceDefinition, new() 
        where TData : notnull
        where TInterface : class, new()
    {
        dataInterfaceFactory.RegisterInterface<TInterfaceDefinition, TData, TInterface>();
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

    public bool RegisterInterface<TInterfaceDefinition, TInterface, TFrontend>()
        where TInterface : TFrontend, new()
        where TInterfaceDefinition : IUserInterfaceDefinition
    {
        userInterfaceStore.AddUserInterfaceImplementation<TInterfaceDefinition, TInterface>();
        return true;
    }

    public void AddNodeToMenu<TNode1, TNode2>(string menuItemName, string? subItemName = null)
        where TNode1 : INode, new()
        where TNode2 : INode, new()
    {
        AddNodeToMenu<TNode1>(menuItemName, subItemName);
        AddNodeToMenu<TNode2>(menuItemName, subItemName);
    }

    public void AddNodeToMenu<TNode1, TNode2, TNode3>(string menuItemName, string? subItemName = null)
        where TNode1 : INode, new()
        where TNode2 : INode, new()
        where TNode3 : INode, new()
    {
        AddNodeToMenu<TNode1>(menuItemName, subItemName);
        AddNodeToMenu<TNode2>(menuItemName, subItemName);
        AddNodeToMenu<TNode3>(menuItemName, subItemName);
    }

    public void AddNodeToMenu<TNode1, TNode2, TNode3, TNode4>(string menuItemName, string? subItemName = null)
        where TNode1 : INode, new()
        where TNode2 : INode, new()
        where TNode3 : INode, new()
        where TNode4 : INode, new()
    {
        AddNodeToMenu<TNode1>(menuItemName, subItemName);
        AddNodeToMenu<TNode2>(menuItemName, subItemName);
        AddNodeToMenu<TNode3>(menuItemName, subItemName);
        AddNodeToMenu<TNode4>(menuItemName, subItemName);
    }

    public void AddNodeToMenu<TNode1, TNode2, TNode3, TNode4, TNode5>(string menuItemName, string? subItemName = null)
        where TNode1 : INode, new()
        where TNode2 : INode, new()
        where TNode3 : INode, new()
        where TNode4 : INode, new()
        where TNode5 : INode, new()
    {
        AddNodeToMenu<TNode1>(menuItemName, subItemName);
        AddNodeToMenu<TNode2>(menuItemName, subItemName);
        AddNodeToMenu<TNode3>(menuItemName, subItemName);
        AddNodeToMenu<TNode4>(menuItemName, subItemName);
        AddNodeToMenu<TNode5>(menuItemName, subItemName);
    }

    public void AddNodeToMenu<TNode1, TNode2, TNode3, TNode4, TNode5, TNode6>(string menuItemName, string? subItemName = null)
        where TNode1 : INode, new()
        where TNode2 : INode, new()
        where TNode3 : INode, new()
        where TNode4 : INode, new()
        where TNode5 : INode, new()
        where TNode6 : INode, new()
    {
        AddNodeToMenu<TNode1>(menuItemName, subItemName);
        AddNodeToMenu<TNode2>(menuItemName, subItemName);
        AddNodeToMenu<TNode3>(menuItemName, subItemName);
        AddNodeToMenu<TNode4>(menuItemName, subItemName);
        AddNodeToMenu<TNode5>(menuItemName, subItemName);
        AddNodeToMenu<TNode6>(menuItemName, subItemName);
    }

    public void AddNodeToMenu<TNode1, TNode2, TNode3, TNode4, TNode5, TNode6, TNode7>(string menuItemName, string? subItemName = null)
        where TNode1 : INode, new()
        where TNode2 : INode, new()
        where TNode3 : INode, new()
        where TNode4 : INode, new()
        where TNode5 : INode, new()
        where TNode6 : INode, new()
        where TNode7 : INode, new()
    {
        AddNodeToMenu<TNode1>(menuItemName, subItemName);
        AddNodeToMenu<TNode2>(menuItemName, subItemName);
        AddNodeToMenu<TNode3>(menuItemName, subItemName);
        AddNodeToMenu<TNode4>(menuItemName, subItemName);
        AddNodeToMenu<TNode5>(menuItemName, subItemName);
        AddNodeToMenu<TNode6>(menuItemName, subItemName);
        AddNodeToMenu<TNode7>(menuItemName, subItemName);
    }

    public void AddNodeToMenu<TNode1, TNode2, TNode3, TNode4, TNode5, TNode6, TNode7, TNode8>(string menuItemName, string? subItemName = null)
        where TNode1 : INode, new()
        where TNode2 : INode, new()
        where TNode3 : INode, new()
        where TNode4 : INode, new()
        where TNode5 : INode, new()
        where TNode6 : INode, new()
        where TNode7 : INode, new()
        where TNode8 : INode, new()
    {
        AddNodeToMenu<TNode1>(menuItemName, subItemName);
        AddNodeToMenu<TNode2>(menuItemName, subItemName);
        AddNodeToMenu<TNode3>(menuItemName, subItemName);
        AddNodeToMenu<TNode4>(menuItemName, subItemName);
        AddNodeToMenu<TNode5>(menuItemName, subItemName);
        AddNodeToMenu<TNode6>(menuItemName, subItemName);
        AddNodeToMenu<TNode7>(menuItemName, subItemName);
        AddNodeToMenu<TNode8>(menuItemName, subItemName);
    }
}
