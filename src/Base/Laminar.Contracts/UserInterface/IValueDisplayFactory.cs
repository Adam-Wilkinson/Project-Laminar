using Laminar.Contracts.Primitives;
using Laminar.PluginFramework.NodeSystem.Contracts;

namespace Laminar.Contracts.UserInterface;

public interface IValueDisplayFactory
{
    IValueDisplay CreateValueDisplay(IValueInfo valueInfo);
}
