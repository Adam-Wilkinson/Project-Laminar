using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.Serialization;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.PluginFramework.Registration;

/// <summary>
/// Defines a host which can have Project: Laminar classes from PlugIns registered with it
/// </summary>
public interface IPluginHost
{
    /// <summary>
    /// Adds a node to a specific menu group
    /// </summary>
    /// <typeparam name="TNode">The type of the <see cref="INode"/></typeparam>
    /// <param name="menuItemName">The name of the root menu to add this to</param>
    /// <param name="subItemName">The name of the sub menu to add this to</param>
    public void AddNodeToMenu<TNode>(string menuItemName, string? subItemName = null) where TNode : INode, new();

    /// <summary>
    /// Registers a node to be automatically used to convert between two types
    /// </summary>
    /// <typeparam name="TInput">The type which is input to the converter</typeparam>
    /// <typeparam name="TOutput">The type which is the output to the converter</typeparam>
    /// <typeparam name="TConverter">The <see cref="INode"/> which is used to convert</typeparam>
    /// <returns></returns>
    public bool TryAddTypeConverter<TInput, TOutput, TConverter>() where TConverter : INode;
    
    public bool RegisterType<T>(
        string hexColour, 
        string userFriendlyName, 
        T defaultValue, 
        IUserInterfaceDefinition defaultEditor, 
        IUserInterfaceDefinition defaultDisplay, 
        TypeSerializer<T>? serializer)
        where T : notnull;

    public bool RegisterDataInterfaceFactory<TInterfaceDefinition, TData, TInterface>(Func<TInterface> factory)
        where TInterfaceDefinition : IUserInterfaceDefinition, new()
        where TData : notnull
        where TInterface : class;
}
