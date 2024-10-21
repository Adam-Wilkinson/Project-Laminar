using System;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Domain;
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

    public IValueInput<T> ValueInput<T>(string valueName, T? initialValue, IUserInterfaceDefinition? editor, IUserInterfaceDefinition? viewer, Action<T>? valueSetter)
    {
        if (initialValue is null && _typeInfoStore.TryGetTypeInfo(typeof(T), out TypeInfo typeInfo))
        {
            initialValue = (T)typeInfo.DefaultValue!;
        }

        ValueInput<T> newInput = new(_uiProvider, _typeInfoStore, valueName, initialValue!);
        newInput.InterfaceDefinition.Editor = editor;
        newInput.InterfaceDefinition.Viewer = viewer;

        if (valueSetter is not null)
        {
            newInput.PreEvaluateAction = () => { valueSetter(newInput.Value); };
        }

        return newInput;
    }

    public IValueOutput<T> ValueOutput<T>(string valueName, T? initialValue, IUserInterfaceDefinition? viewer, IUserInterfaceDefinition? editor, bool isUserEditable, Func<T>? getter)
    {
        if (initialValue is null && _typeInfoStore.TryGetTypeInfo(typeof(T), out TypeInfo typeInfo))
        {
            initialValue = (T)typeInfo.DefaultValue!;
        }

        ValueOutput<T> newOutput = new(_uiProvider, _typeInfoStore, valueName, initialValue!);
        newOutput.InterfaceDefinition.Viewer = viewer;
        newOutput.InterfaceDefinition.Editor = editor;
        newOutput.InterfaceDefinition.IsUserEditable = isUserEditable;

        if (getter is not null)
        {
            newOutput.GetterOverride = getter;
        }

        return newOutput;
    }
}
