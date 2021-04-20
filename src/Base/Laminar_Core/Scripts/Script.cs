using Laminar_PluginFramework.Primitives;

namespace Laminar_Core.Scripts
{
    public class Script : IScript
    {
        public Script(ScriptDependencyAggregate deps)
        {
            (IsActive, Name) = deps;
            IsActive.Value = true;
        }

        public IObservableValue<string> Name { get; }

        public IObservableValue<bool> IsActive { get; }
    }
}
