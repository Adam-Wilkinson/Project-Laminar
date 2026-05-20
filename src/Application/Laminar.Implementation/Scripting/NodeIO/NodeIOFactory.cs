using System;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Domain.Exceptions;
using Laminar.Implementation.Base.UserInterface;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.IO;
using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Scripting.NodeIO;

internal class NodeIOFactory(ITypeInfoStore typeInfoStore) : INodeIOFactory
{
    public IValueInput<T> ValueInput<T>(string valueName, T? initialValue, IUserInterfaceDefinition? editor, IUserInterfaceDefinition? viewer, Action<T>? valueSetter)
        where T : notnull
    {
        initialValue ??=
            typeInfoStore.TryGetTypeInfo(typeof(T), out var typeInfo) && typeInfo.DefaultValue is T defaultInitialValue
                ? defaultInitialValue
                : throw new TypeNotRegisteredException(typeof(T));

        ValueInput<T> newInput = new(typeInfoStore, valueName, initialValue)
        {
            InterfaceData = new SourcedInterfaceData<T>(initialValue)
            {
                Name = valueName,
                Editor = editor,
                Viewer = viewer,
            }
        };

        if (valueSetter is not null)
        {
            newInput.PreEvaluateAction = () => { valueSetter(newInput.InterfaceData.Value); };
        }

        return newInput;
    }

    public IValueOutput<T> ValueOutput<T>(string valueName, T? initialValue, IUserInterfaceDefinition? viewer, IUserInterfaceDefinition? editor, bool isUserEditable, Func<T>? getter)
        where T : notnull
    {
        initialValue ??=
            typeInfoStore.TryGetTypeInfo(typeof(T), out var typeInfo) && typeInfo.DefaultValue is T defaultInitialValue
                ? defaultInitialValue
                : throw new TypeNotRegisteredException(typeof(T));

        SourcedInterfaceData<T> outputInterface = new(initialValue)
        {
            Name = valueName,
            Editor = editor,
            Viewer = viewer,
        };

        if (getter is not null)
        {
            outputInterface.ValueProvider = new FuncValueProvider<T>(getter);
        }

        ValueOutput<T> newOutput = new(typeInfoStore, initialValue, valueName, outputInterface);

        return newOutput;
    }
}
