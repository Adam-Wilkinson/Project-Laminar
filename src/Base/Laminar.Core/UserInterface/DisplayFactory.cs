using Laminar.Contracts.UserInterface;
using Laminar.PluginFramework.NodeSystem.Contracts;

namespace Laminar.Core.UserInterface;

internal class DisplayFactory : IDisplayFactory
{
    private readonly IUserInterfaceProvider _userInterfaceProvider;

    public DisplayFactory(IUserInterfaceProvider userInterfaceProvider)
    {
        _userInterfaceProvider = userInterfaceProvider;
    }

    public IDisplay CreateDisplayForValue(IValueInfo valueInfo)
    {
        return new Display(valueInfo, _userInterfaceProvider);
    }
}
