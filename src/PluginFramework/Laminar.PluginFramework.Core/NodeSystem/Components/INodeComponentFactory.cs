using Laminar.PluginFramework.NodeSystem.IO;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.PluginFramework.NodeSystem.Components;

public interface INodeComponentFactory
{
    public INodeComponentCloner<T> Cloner<T>(Func<T> componentGenerator, int initialComponentCount) where T : INodeComponent;

    public INodeRow<T> CreateSingleRow<T>(IInput? input, T interfaceData, IOutput? output) where T : IInterfaceData;
}
