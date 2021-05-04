using Laminar_PluginFramework.Primitives;

namespace Laminar_Core.Scripting
{
    public class ScriptInstance : IScriptInstance
    {
        public ScriptInstance(ScriptDependencyAggregate deps)
        {
            (IsActive, Name) = deps;
            IsActive.Value = true;
        }

        public IObservableValue<string> Name { get; }

        public IObservableValue<bool> IsActive { get; }
    }
}
