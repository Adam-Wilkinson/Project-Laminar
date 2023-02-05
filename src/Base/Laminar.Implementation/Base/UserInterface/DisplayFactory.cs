using Laminar.Contracts.Base.UserInterface;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.Implementation.Base.UserInterface;

internal class DisplayFactory : IDisplayFactory
{
    private readonly IUserInterfaceProvider _userInterfaceProvider;

    public DisplayFactory(IUserInterfaceProvider userInterfaceProvider)
    {
        _userInterfaceProvider = userInterfaceProvider;
    }

    public IDisplay CreateDisplayForValue(IDisplayValue valueInfo)
    {
        return new Display(valueInfo, _userInterfaceProvider);
    }
}
