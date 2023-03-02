using System;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.UserInterface;
using Laminar.PluginFramework.NodeSystem.IO;
using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Scripting.NodeIO;

internal class NodeIOFactory : INodeIOFactory
{
    private readonly IUserInterfaceProvider _uiProvider;
    private readonly ITypeInfoStore _typeInfoStore;

    public NodeIOFactory(IUserInterfaceProvider uiProvider, ITypeInfoStore typeInfoStore)
    {
        _uiProvider = uiProvider;
        _typeInfoStore = typeInfoStore;
    }

    public IValueInput<T> ValueInput<T>(string valueName, T initialValue, IUserInterfaceDefinition? editor, IUserInterfaceDefinition? viewer, Action<T>? valueSetter)
    {
        ValueInput<T> output = new(_uiProvider, _typeInfoStore, valueName, initialValue);
        output.InterfaceDefinition.Editor = editor;
        output.InterfaceDefinition.Viewer = viewer;

        if (valueSetter is not null)
        {
            output.PreEvaluateAction = () => { valueSetter(output.Value); };
        }

        return output;
    }

    public IValueOutput<T> ValueOutput<T>(string valueName, T initialValue, IUserInterfaceDefinition? viewer, IUserInterfaceDefinition? editor, bool isUserEditable)
    {
        ValueOutput<T> output = new(_uiProvider, _typeInfoStore, valueName, initialValue);
        output.InterfaceDefinition.Viewer = viewer;
        output.InterfaceDefinition.Editor = editor;
        output.InterfaceDefinition.IsUserEditable = isUserEditable;
        return output;
    }
}
