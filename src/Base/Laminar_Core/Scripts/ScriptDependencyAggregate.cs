using Laminar_PluginFramework.Primitives;

namespace Laminar_Core.Scripts
{
    public class ScriptDependencyAggregate
    {
        public ScriptDependencyAggregate(IObservableValue<bool> isActive, IObservableValue<string> name)
        {
            IsActive = isActive;
            Name = name;
        }


        public IObservableValue<bool> IsActive { get; }

        public IObservableValue<string> Name { get; }

        public void Deconstruct(out IObservableValue<bool> IsActive, out IObservableValue<string> Name)
        {
            IsActive = this.IsActive;
            Name = this.Name;
        }
    }
}
