using Laminar_PluginFramework.UserInterfaces;

namespace Laminar.Contracts.UserInterface;

public interface IUserInterfaceProvider
{
    bool InterfaceImplemented(IUserInterfaceDefinition interfaceDefinition, string frontendKey);

    object GetUserInterface(IUserInterfaceDefinition definition, string frontendKey);

    IUserInterfaceDefinition GetDefaultDefinition(Type valueType, bool getEditor);
}
