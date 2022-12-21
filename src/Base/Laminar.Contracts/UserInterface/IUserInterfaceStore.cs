using Laminar_PluginFramework.UserInterfaces;

namespace Laminar.Contracts.UserInterface;

public interface IUserInterfaceStore : IReadOnlyUserInterfaceStore
{
    bool AddUserInterfaceImplementation<TDefinition, TInterface>() 
        where TDefinition : IUserInterfaceDefinition 
        where TInterface : new();
}
