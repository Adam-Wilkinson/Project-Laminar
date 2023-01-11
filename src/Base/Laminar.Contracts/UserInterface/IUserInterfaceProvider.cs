using Laminar.PluginFramework.NodeSystem.Contracts;
using Laminar.PluginFramework.UserInterfaces;

namespace Laminar.Contracts.UserInterface;

public interface IUserInterfaceProvider
{
    bool InterfaceImplemented(IUserInterfaceDefinition interfaceDefinition);

    object GetUserInterface(IUserInterfaceDefinition definition);

    IUserInterfaceDefinition FindDefinitionForValueInfo(IValueInfo valueInfo);
}
