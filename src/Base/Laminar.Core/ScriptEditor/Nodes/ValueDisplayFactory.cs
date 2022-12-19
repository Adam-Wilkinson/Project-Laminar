using Laminar.Contracts.Primitives;
using Laminar.Contracts.UserInterface;
using Laminar.PluginFramework.NodeSystem.Contracts;

namespace Laminar.Core.ScriptEditor.Nodes;

public class ValueDisplayFactory : IValueDisplayFactory
{
    readonly IUserInterfaceFactory _uiProvider;

    public ValueDisplayFactory(IUserInterfaceFactory _userInterfaceProvider)
    {
        _uiProvider = _userInterfaceProvider;
    }

    public IValueDisplay CreateValueDisplay(IValueInfo valueInfo)
    {
        return new ValueDisplay(valueInfo, _uiProvider);
    }
}