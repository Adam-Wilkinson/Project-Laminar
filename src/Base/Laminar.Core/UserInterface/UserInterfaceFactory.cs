using Laminar.Contracts.Primitives;
using Laminar.Contracts.UserInterface;
using Laminar.PluginFramework.NodeSystem.Contracts;

namespace Laminar.Core.UserInterface;

internal class UserInterfaceFactory : IUserInterfaceFactory
{
    private readonly IUserInterfaceProvider _interfaceProvider;

    public UserInterfaceFactory(IUserInterfaceProvider interfaceProvider)
    {
        _interfaceProvider = interfaceProvider;
    }

    public IUserInterface CreateUserInterface(IValueInfo valueInfo, string frontendKey)
    {
        return new UserInterface(valueInfo, frontendKey, _interfaceProvider);
    }
}