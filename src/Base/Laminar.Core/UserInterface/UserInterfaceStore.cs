using System;
using System.Collections.Generic;
using Laminar.Contracts.UserInterface;
using Laminar_PluginFramework.UserInterfaces;

namespace Laminar.Core.UserInterface;

internal class UserInterfaceStore : IUserInterfaceStore
{
    private readonly Dictionary<string, SingleFrontendUserInterfaceStore> _frontends = new();

    public bool AddUserInterfaceImplementation<TDefinition, TFrontend, TUserInterface>()
        where TDefinition : IUserInterfaceDefinition
        where TUserInterface : TFrontend, new()
    {
        return GetFrontend(typeof(TFrontend).FullName).RegisterUserInterface<TDefinition, TUserInterface>();
    }

    public bool HasFrontendOfType(string frontendKey) => frontendKey is not null && _frontends.ContainsKey(frontendKey);
    public bool HasImplementation(string frontendKey, Type definitionType) => HasFrontendOfType(frontendKey) && _frontends[frontendKey].HasUserInterfaceOfType(definitionType);
    public bool TryGetUserInterface(string frontendKey, Type definitionType, out Type? userInterfaceType) => _frontends[frontendKey].TryGetUserInterfaceType(definitionType, out userInterfaceType);

    private SingleFrontendUserInterfaceStore GetFrontend(string frontendKey)
    {
        if (_frontends.TryGetValue(frontendKey, out SingleFrontendUserInterfaceStore frontend))
        {
            return frontend;
        }

        SingleFrontendUserInterfaceStore newFrontend = new();
        _frontends.Add(frontendKey, newFrontend);
        return newFrontend;
    }

    private class SingleFrontendUserInterfaceStore
    {
        private readonly Dictionary<Type, Type> _userInterfaceTypesByDefinition = new();

        public bool TryGetUserInterfaceType(Type definitionType, out Type? userInterfaceType) => _userInterfaceTypesByDefinition.TryGetValue(definitionType, out userInterfaceType);

        public bool RegisterUserInterface<TDefinition, TUserInterface>() => _userInterfaceTypesByDefinition.TryAdd(typeof(TDefinition), typeof(TUserInterface));

        public bool HasUserInterfaceOfType(Type definitionType) => _userInterfaceTypesByDefinition.ContainsKey(definitionType);
    }
}