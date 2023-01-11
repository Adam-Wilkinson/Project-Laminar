using System;
using Laminar.Domain;
using Laminar.PluginFramework.Registration;
using Laminar.PluginFramework.Serialization;
using Laminar.PluginFramework.UserInterfaces;
using Laminar.Contracts.Scripting.Connection;
using Laminar.PluginFramework.NodeSystem.Contracts.IO;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;
using Laminar.PluginFramework.NodeSystem;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Base;

namespace Laminar.Implementation.Base.PluginLoading;

public class PluginHost : IPluginHost
{
    private readonly IRegisteredPlugin _registeredPlugin;
    private readonly ITypeInfoStore _typeInfoStore;
    private readonly ILoadedNodeManager _loadedNodeManager;
    private readonly IUserInterfaceStore _userInterfaceStore;
    private readonly IConnectorViewFactory _connectorFactory;

    public PluginHost(IRegisteredPlugin registeredPlugin, ITypeInfoStore typeInfoStore, ILoadedNodeManager loadedNodeManager, IUserInterfaceStore userInterfaceStore, IConnectorViewFactory connectorFactory)
    {
        _registeredPlugin = registeredPlugin;
        _typeInfoStore = typeInfoStore;
        _loadedNodeManager = loadedNodeManager;
        _userInterfaceStore = userInterfaceStore;
        _connectorFactory = connectorFactory;

        StaticRegistrations.Register(this);
    }

    public void AddNodeToMenu<TNode>(string menuItemName, string subItemName = null) where TNode : INode, new()
    {
        TNode node = new();
        _registeredPlugin.RegisterNode(node);
        _loadedNodeManager.AddNodeToCatagory(node, subItemName is null ? menuItemName : $"{menuItemName}{ItemCatagory<IWrappedNode>.SeparationChar}{subItemName}");
    }

    public bool RegisterType<T>(string hexColour, string userFriendlyName, T defaultValue, IUserInterfaceDefinition defaultEditor, IUserInterfaceDefinition defaultDisplay, IObjectSerializer<T> serializer)
    {
        if (serializer is not null)
        {
            // _instance.Serializer.RegisterSerializer(serializer);
        }

        _typeInfoStore.RegisterType(typeof(T), new TypeInfo(userFriendlyName, defaultEditor, defaultDisplay, hexColour, defaultValue));
        return true;
    }

    public bool TryAddTypeConverter<TInput, TOutput, TConverter>() where TConverter : INode
        => throw new NotImplementedException();

    public bool RegisterInterface<TInterfaceDefinition, TInterface, TFrontend>()
        where TInterface : TFrontend, new()
        where TInterfaceDefinition : IUserInterfaceDefinition
    {
        _userInterfaceStore.AddUserInterfaceImplementation<TInterfaceDefinition, TInterface>();
        return true;
    }

    public void AddNodeToMenu<TNode1, TNode2>(string menuItemName, string subItemName = null)
        where TNode1 : INode, new()
        where TNode2 : INode, new()
    {
        AddNodeToMenu<TNode1>(menuItemName, subItemName);
        AddNodeToMenu<TNode2>(menuItemName, subItemName);
    }

    public void AddNodeToMenu<TNode1, TNode2, TNode3>(string menuItemName, string subItemName = null)
        where TNode1 : INode, new()
        where TNode2 : INode, new()
        where TNode3 : INode, new()
    {
        AddNodeToMenu<TNode1>(menuItemName, subItemName);
        AddNodeToMenu<TNode2>(menuItemName, subItemName);
        AddNodeToMenu<TNode3>(menuItemName, subItemName);
    }

    public void AddNodeToMenu<TNode1, TNode2, TNode3, TNode4>(string menuItemName, string subItemName = null)
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

    public void AddNodeToMenu<TNode1, TNode2, TNode3, TNode4, TNode5>(string menuItemName, string subItemName = null)
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

    public void AddNodeToMenu<TNode1, TNode2, TNode3, TNode4, TNode5, TNode6>(string menuItemName, string subItemName = null)
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

    public void AddNodeToMenu<TNode1, TNode2, TNode3, TNode4, TNode5, TNode6, TNode7>(string menuItemName, string subItemName = null)
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

    public void AddNodeToMenu<TNode1, TNode2, TNode3, TNode4, TNode5, TNode6, TNode7, TNode8>(string menuItemName, string subItemName = null)
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

    public bool RegisterInputConnector<TNodeInput, TNodeInputConnector>()
        where TNodeInput : IInput
        where TNodeInputConnector : IInputConnector<TNodeInput>
    {
        _connectorFactory.RegisterInputConnector<TNodeInput, TNodeInputConnector>();
        return true;
    }

    public bool RegisterOutputConnector<TNodeOutput, TNodeOutputConnector>()
        where TNodeOutput : IOutput
        where TNodeOutputConnector : IOutputConnector<TNodeOutput>
    {
        _connectorFactory.RegisterOutputConnector<TNodeOutput, TNodeOutputConnector>();
        return true;
    }
}
