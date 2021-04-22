using Laminar_Core.NodeSystem.NodeTreeSystem;
using Laminar_PluginFramework.Primitives;
using System.Collections.ObjectModel;

namespace Laminar_Core.Scripts
{
    public class ScriptCollection : IScriptCollection
    {
        private readonly IObjectFactory _factory;

        public ScriptCollection(IObjectFactory factory)
        {
            _factory = factory;
        }

        public ObservableCollection<IScriptInstance> Scripts { get; } = new();
    }
}
