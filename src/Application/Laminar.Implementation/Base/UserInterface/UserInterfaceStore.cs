using System;
using System.Collections.Generic;
using System.Text;
using Laminar.Contracts.Base.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Base.UserInterface;

internal class UserInterfaceStore : IUserInterfaceStore
{
    private readonly Dictionary<Type, Type> _userInterfacesByTypeDefinition = [];

    public bool AddUserInterfaceImplementation<TDefinition, TUserInterface>()
        where TDefinition : IUserInterfaceDefinition
        where TUserInterface : new()
    {
        return _userInterfacesByTypeDefinition.TryAdd(typeof(TDefinition), typeof(TUserInterface));
    }

    public bool HasImplementation(Type definitionType) => _userInterfacesByTypeDefinition.ContainsKey(definitionType);
    public bool TryGetUserInterface(Type definitionType, out Type userInterfaceType) 
    { 
        if (_userInterfacesByTypeDefinition.TryGetValue(definitionType, out var interfaceType) && interfaceType is not null)
        {
            userInterfaceType = interfaceType;
            return true;
        }

        userInterfaceType = typeof(object);
        return false;
    }
}