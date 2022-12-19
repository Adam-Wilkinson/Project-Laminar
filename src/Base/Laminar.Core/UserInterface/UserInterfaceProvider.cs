using System;
using Laminar.Contracts.UserInterface;
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

    public IUserInterfaceDefinition GetDefaultDefinition(Type valueType, bool getEditor)
    {
        if (getEditor && _typeInfoStore.GetTypeInfo(valueType).EditorDefinition is IUserInterfaceDefinition editorDefinition)
        {
            return editorDefinition;
        }

        if (!getEditor && _typeInfoStore.GetTypeInfo(valueType).ViewerDefinition is IUserInterfaceDefinition viewerDefinition)
        {
            return viewerDefinition;
        }

        return new DefaultViewer();
    }

    public object GetUserInterface(IUserInterfaceDefinition definition, string frontendTypeKey)
    {
        if (!_store.HasFrontendOfType(frontendTypeKey))
        {
            throw new ArgumentException(nameof(frontendTypeKey));
        }

        if (definition is not null && _store.TryGetUserInterface(frontendTypeKey, definition.GetType(), out Type userInterface))
        {
            return Activator.CreateInstance(userInterface);
        }

        if (_store.TryGetUserInterface(frontendTypeKey, typeof(DefaultViewer), out Type defaultViewer))
        {
            return Activator.CreateInstance(defaultViewer);
        }

        throw new ApplicationException("Frontend doesn't have a default viewer");
    }

    public bool InterfaceImplemented(IUserInterfaceDefinition interfaceDefinition, string frontendKey)
    {
        return _store.HasImplementation(frontendKey, interfaceDefinition.GetType());
    }
}