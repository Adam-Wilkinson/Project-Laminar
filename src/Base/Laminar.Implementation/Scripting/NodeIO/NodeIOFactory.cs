using Laminar.Contracts.Base;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Implementation.Scripting.Connections;
using Laminar.PluginFramework.NodeSystem.IO;
using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.Implementation.Scripting.NodeIO;

internal class NodeIOFactory : INodeIOFactory
{
    private readonly IUserInterfaceDefinitionFinder _uiFinder;
    private readonly ITypeInfoStore _typeInfoStore;

    public NodeIOFactory(IUserInterfaceDefinitionFinder uiFinder, ITypeInfoStore typeInfoStore)
    {
        _uiFinder = uiFinder;
        _typeInfoStore = typeInfoStore;
    }

    public IValueInput<T> ValueInput<T>(string valueName, T initialValue) => new ValueInput<T>(
        _uiFinder, 
        _typeInfoStore,
        valueName,
        initialValue);
    public IValueOutput<T> ValueOutput<T>(string valueName, T initialValue) => new ValueOutput<T>(
        _uiFinder,
        _typeInfoStore,
        valueName,
        initialValue);
}
