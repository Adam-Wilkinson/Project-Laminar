using Laminar_PluginFramework.Primitives;

namespace Laminar_Core.Scripting
{
    public interface IScriptInstance
    {
        IObservableValue<string> Name { get; }

        IObservableValue<bool> IsActive { get; }
    }
}
