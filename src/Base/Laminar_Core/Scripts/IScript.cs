using Laminar_PluginFramework.Primitives;

namespace Laminar_Core.Scripts
{
    public interface IScript
    {
        IObservableValue<string> Name { get; }

        IObservableValue<bool> IsActive { get; }
    }
}
