using Laminar.Contracts.Primitives;
using Laminar.PluginFramework.NodeSystem.Contracts;

namespace Laminar.Contracts.UserInterface;

public interface IUserInterfaceFactory
{
    public IUserInterface CreateUserInterface(IValueInfo valueInfo, string frontendKey);
}
