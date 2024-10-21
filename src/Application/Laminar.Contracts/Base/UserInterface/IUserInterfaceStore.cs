using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Contracts.Base.UserInterface;

public interface IUserInterfaceStore : IReadOnlyUserInterfaceStore
{
    bool AddUserInterfaceImplementation<TDefinition, TInterface>()
        where TDefinition : IUserInterfaceDefinition
        where TInterface : new();
}
