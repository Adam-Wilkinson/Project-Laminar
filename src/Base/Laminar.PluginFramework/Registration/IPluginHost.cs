using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;
using Laminar.PluginFramework.NodeSystem.Contracts.IO;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.Serialization;
using Laminar.PluginFramework.UserInterfaces;

namespace Laminar.PluginFramework.Registration;

/// <summary>
/// Defines a host which can have OpenFlow classes from PlugIns registered with it
/// </summary>
public interface IPluginHost
{
    /// <summary>
    /// Adds a node to a specific menu group
    /// </summary>
    /// <typeparam name="TNode">The type of the <see cref="IFunctionNode"/></typeparam>
    /// <param name="menuItemName">The name of the root menu to add this to</param>
    /// <param name="subItemName">The name of the sub menu to add this to</param>
    void AddNodeToMenu<TNode>(string menuItemName, string subItemName = null)
        where TNode : INode, new();

    /// <summary>
    /// Adds two nodes to a specific menu group
    /// </summary>
    /// <typeparam name="TNode1">The type of the first <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode2">The type of the second <see cref="IFunctionNode"/></typeparam>
    /// <param name="menuItemName">The name of the root menu to add them to</param>
    /// <param name="subItemName">The name of the sub menu to add them to</param>
    void AddNodeToMenu<TNode1, TNode2>(string menuItemName, string subItemName = null)
        where TNode1 : INode, new()
        where TNode2 : INode, new();

    /// <summary>
    /// Adds three nodes to a specific menu group
    /// </summary>
    /// <typeparam name="TNode1">The type of the first <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode2">The type of the second <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode3">The type of the third <see cref="IFunctionNode"/></typeparam>
    /// <param name="menuItemName">The name of the root menu to add them to</param>
    /// <param name="subItemName">The name of the sub menu to add them to</param>
    void AddNodeToMenu<TNode1, TNode2, TNode3>(string menuItemName, string subItemName = null)
        where TNode1 : INode, new()
        where TNode2 : INode, new()
        where TNode3 : INode, new();

    /// <summary>
    /// Adds four nodes to a specific menu group
    /// </summary>
    /// <typeparam name="TNode1">The type of the first <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode2">The type of the second <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode3">The type of the third <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode4">The type of the fourth <see cref="IFunctionNode"/></typeparam>
    /// <param name="menuItemName">The name of the root menu to add them to</param>
    /// <param name="subItemName">The name of the sub menu to add them to</param>
    void AddNodeToMenu<TNode1, TNode2, TNode3, TNode4>(string menuItemName, string subItemName = null)
        where TNode1 : INode, new()
        where TNode2 : INode, new()
        where TNode3 : INode, new()
        where TNode4 : INode, new();

    /// <summary>
    /// Adds five nodes to a specific menu group
    /// </summary>
    /// <typeparam name="TNode1">The type of the first <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode2">The type of the second <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode3">The type of the third <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode4">The type of the fourth <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode5">The type of the fifth <see cref="IFunctionNode"/></typeparam>
    /// <param name="menuItemName">The name of the root menu to add them to</param>
    /// <param name="subItemName">The name of the sub menu to add them to</param>
    void AddNodeToMenu<TNode1, TNode2, TNode3, TNode4, TNode5>(string menuItemName, string subItemName = null)
        where TNode1 : INode, new()
        where TNode2 : INode, new()
        where TNode3 : INode, new()
        where TNode4 : INode, new()
        where TNode5 : INode, new();

    /// <summary>
    /// Adds six nodes to a specific menu group
    /// </summary>
    /// <typeparam name="TNode1">The type of the first <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode2">The type of the second <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode3">The type of the third <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode4">The type of the fourth <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode5">The type of the fifth <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode6">The type of the sixth <see cref="IFunctionNode"/></typeparam>
    /// <param name="menuItemName">The name of the root menu to add them to</param>
    /// <param name="subItemName">The name of the sub menu to add them to</param>
    void AddNodeToMenu<TNode1, TNode2, TNode3, TNode4, TNode5, TNode6>(string menuItemName, string subItemName = null)
        where TNode1 : INode, new()
        where TNode2 : INode, new()
        where TNode3 : INode, new()
        where TNode4 : INode, new()
        where TNode5 : INode, new()
        where TNode6 : INode, new();

    /// <summary>
    /// Adds seven nodes to a specific menu group
    /// </summary>
    /// <typeparam name="TNode1">The type of the first <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode2">The type of the second <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode3">The type of the third <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode4">The type of the fourth <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode5">The type of the fifth <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode6">The type of the sixth <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode7">The type of the seventh <see cref="IFunctionNode"/></typeparam>
    /// <param name="menuItemName">The name of the root menu to add them to</param>
    /// <param name="subItemName">The name of the sub menu to add them to</param>
    void AddNodeToMenu<TNode1, TNode2, TNode3, TNode4, TNode5, TNode6, TNode7>(string menuItemName, string subItemName = null)
        where TNode1 : INode, new()
        where TNode2 : INode, new()
        where TNode3 : INode, new()
        where TNode4 : INode, new()
        where TNode5 : INode, new()
        where TNode6 : INode, new()
        where TNode7 : INode, new();

    /// <summary>
    /// Adds eight nodes to a specific menu group
    /// </summary>
    /// <typeparam name="TNode1">The type of the first <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode2">The type of the second <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode3">The type of the third <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode4">The type of the fourth <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode5">The type of the fifth <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode6">The type of the sixth <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode7">The type of the seventh <see cref="IFunctionNode"/></typeparam>
    /// <typeparam name="TNode8">The type of the eighth <see cref="IFunctionNode"/></typeparam>
    /// <param name="menuItemName">The name of the root menu to add them to</param>
    /// <param name="subItemName">The name of the sub menu to add them to</param>
    void AddNodeToMenu<TNode1, TNode2, TNode3, TNode4, TNode5, TNode6, TNode7, TNode8>(string menuItemName, string subItemName = null)
        where TNode1 : INode, new()
        where TNode2 : INode, new()
        where TNode3 : INode, new()
        where TNode4 : INode, new()
        where TNode5 : INode, new()
        where TNode6 : INode, new()
        where TNode7 : INode, new()
        where TNode8 : INode, new();

    /// <summary>
    /// Registers a node to be automatically used to convert between two types
    /// </summary>
    /// <typeparam name="TInput">The type which is input to the converter</typeparam>
    /// <typeparam name="TOutput">The type which is the output to the converter</typeparam>
    /// <typeparam name="TConverter">The <see cref="IFunctionNode"/> which is used to convert</typeparam>
    /// <returns></returns>
    bool TryAddTypeConverter<TInput, TOutput, TConverter>()
        where TConverter : INode;

    bool RegisterInterface<TInterfaceDefinition, TInterface, TFrontend>()
        where TInterfaceDefinition : IUserInterfaceDefinition
        where TInterface : TFrontend, new();

    bool RegisterInputConnector<TNodeInput, TNodeInputConnector>()
        where TNodeInput : IInput
        where TNodeInputConnector : IInputConnector<TNodeInput>;

    bool RegisterOutputConnector<TNodeOutput, TNodeOutputConnector>()
        where TNodeOutput : IOutput
        where TNodeOutputConnector : IOutputConnector<TNodeOutput>;

    bool RegisterType<T>(string hexColour, string userFriendlyName, T defaultValue, IUserInterfaceDefinition defaultEditor, IUserInterfaceDefinition defaultDisplay, IObjectSerializer<T> serializer);
}
