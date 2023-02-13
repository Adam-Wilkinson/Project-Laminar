using Laminar.Contracts.Base;
using Laminar.PluginFramework.NodeSystem.IO;
using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

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

    public IValueInput<T> ValueInput<T>(string valueName, T initialValue, IUserInterfaceDefinition? editor, IUserInterfaceDefinition? viewer) {
        ValueInput<T> output = new(_uiFinder, _typeInfoStore, valueName, initialValue);
        output.ValueUserInterface.Editor = editor;
        output.ValueUserInterface.Viewer = viewer;
        return output;
    }

    public IValueOutput<T> ValueOutput<T>(string valueName, T initialValue, IUserInterfaceDefinition? viewer, IUserInterfaceDefinition? editor, bool isUserEditable)
    {
        ValueOutput<T> output = new(_uiFinder, _typeInfoStore, valueName, initialValue);
        output.ValueUserInterface.Viewer = viewer;
        return output;
    }
}
