using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.PluginFramework.UserInterface;

public interface IUserInterfaceDefinitionFinder
{
    public IUserInterfaceDefinition? GetCurrentDefinitionOf(ValueInterfaceDefinition valueInterfaceDefinition);
}
