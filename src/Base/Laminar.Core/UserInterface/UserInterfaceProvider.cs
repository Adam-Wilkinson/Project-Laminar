using System;
using Laminar.Contracts.UserInterface;
using Laminar.Core.UserPreferences;
using Laminar.PluginFramework.NodeSystem.Contracts;
using Laminar_PluginFramework.UserInterfaces;

namespace Laminar.Core.UserInterface;

public class UserInterfaceProvider : IUserInterfaceProvider
{  
    readonly IReadOnlyUserInterfaceStore _store;
    readonly IReadonlyTypeInfoStore _typeInfoStore;

    public UserInterfaceProvider(IReadOnlyUserInterfaceStore interfaceStore, IReadonlyTypeInfoStore typeInfoStore)
    {
        _store = interfaceStore;
        _typeInfoStore = typeInfoStore;
    }

    public IUserInterfaceDefinition FindDefinitionForValueInfo(IValueInfo valueInfo)
    {
        if (valueInfo.IsUserEditable && valueInfo.Editor is not null && InterfaceImplemented(valueInfo.Editor))
        {
            return valueInfo.Editor;
        }

        if (!valueInfo.IsUserEditable && valueInfo.Viewer is not null && InterfaceImplemented(valueInfo.Viewer))
        {
            return valueInfo.Viewer;
        }

        if (valueInfo.IsUserEditable && _typeInfoStore.GetTypeInfo(valueInfo.ValueType).EditorDefinition is IUserInterfaceDefinition editorDefinition && InterfaceImplemented(editorDefinition))
        {
            return editorDefinition;
        }

        if (!valueInfo.IsUserEditable && _typeInfoStore.GetTypeInfo(valueInfo.ValueType).ViewerDefinition is IUserInterfaceDefinition viewerDefinition && InterfaceImplemented(viewerDefinition))
        {
            return viewerDefinition;
        }

        return new DefaultViewer();
    }

    public object? GetUserInterface(IUserInterfaceDefinition definition)
    {
        if (definition is not null && _store.TryGetUserInterface(definition.GetType(), out Type userInterface))
        {
            return Activator.CreateInstance(userInterface);
        }

        if (_store.TryGetUserInterface(typeof(DefaultViewer), out Type defaultViewer))
        {
            return Activator.CreateInstance(defaultViewer);
        }

        return null;
    }

    public bool InterfaceImplemented(IUserInterfaceDefinition interfaceDefinition)
    {
        return _store.HasImplementation(interfaceDefinition.GetType());
    }
}