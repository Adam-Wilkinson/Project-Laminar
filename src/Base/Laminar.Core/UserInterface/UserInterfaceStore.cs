using System;
using System.Collections.Generic;
using Laminar.Contracts.UserInterface;
using Laminar.PluginFramework.UserInterfaces;

namespace Laminar.Core.UserInterface;

internal class UserInterfaceStore : IUserInterfaceStore
{
    private readonly Dictionary<Type, Type> _userInterfacesByTypeDefinition = new();

    public bool AddUserInterfaceImplementation<TDefinition, TUserInterface>()
        where TDefinition : IUserInterfaceDefinition
        where TUserInterface : new()
    {
        return _userInterfacesByTypeDefinition.TryAdd(typeof(TDefinition), typeof(TUserInterface));
    }

    public bool HasImplementation(Type definitionType) => _userInterfacesByTypeDefinition.ContainsKey(definitionType);
    public bool TryGetUserInterface(Type definitionType, out Type? userInterfaceType) => _userInterfacesByTypeDefinition.TryGetValue(definitionType, out userInterfaceType);
}