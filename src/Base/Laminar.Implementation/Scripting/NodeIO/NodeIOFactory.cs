using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.PluginFramework.NodeSystem.IO;
using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.Implementation.Scripting.NodeIO;

internal class NodeIOFactory : INodeIOFactory
{
    private readonly IUserInterfaceDefinitionFinder _uiFinder;

    public NodeIOFactory(IUserInterfaceDefinitionFinder uiFinder)
    {
        _uiFinder = uiFinder;
    }

    public IValueInput<T> ValueInput<T>(string valueName, T initialValue) => new ValueInput<T>(
        _uiFinder, 
        valueName,
        initialValue);
    public IValueOutput<T> ValueOutput<T>(string valueName, T initialValue) => new ValueOutput<T>(
        _uiFinder,
        valueName,
        initialValue);
}
