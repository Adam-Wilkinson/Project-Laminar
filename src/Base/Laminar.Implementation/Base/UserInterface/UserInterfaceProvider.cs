using System;
using Laminar.Contracts.Base.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Base.UserInterface;

public class UserInterfaceProvider : IUserInterfaceProvider
{
    readonly IReadOnlyUserInterfaceStore _store;

    public UserInterfaceProvider(IReadOnlyUserInterfaceStore interfaceStore)
    {
        _store = interfaceStore;
    }

    public object GetUserInterface(IUserInterfaceDefinition definition)
    {
        if (definition is not null 
            && _store.TryGetUserInterface(definition.GetType(), out Type userInterface)
            && Activator.CreateInstance(userInterface) is object obj)
        {
            return obj;
        }

        if (_store.TryGetUserInterface(typeof(DefaultViewer), out Type defaultViewer)
            && Activator.CreateInstance(defaultViewer) is object defaultObj)
        {
            return defaultObj;
        }

        return null;
    }

    public bool InterfaceImplemented(IUserInterfaceDefinition interfaceDefinition)
    {
        return _store.HasImplementation(interfaceDefinition.GetType());
    }
}