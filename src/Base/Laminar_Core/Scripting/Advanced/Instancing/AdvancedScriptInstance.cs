using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Scripting.Advanced.Editing;

namespace Laminar_Core.Scripting.Advanced.Instancing
{
    public class AdvancedScriptInstance : ScriptInstance, IAdvancedScriptInstance
    {
        private readonly INodeFactory _nodeFactory;
        private IAdvancedScript _script;

        public AdvancedScriptInstance(ScriptDependencyAggregate deps, INodeFactory nodeFactory) : base(deps)
        {
            IsActive.Value = false;
            _nodeFactory = nodeFactory;
        }

        public INodeContainer Inputs { get; private set; }

        public IAdvancedScript Script
        {
            get => _script;
            set
            {
                _script = value;
                AdvancedScriptInputsNode inputs = new();
                inputs.SetInstance(this);
                inputs.BindToInputs(_script.Inputs);
                inputs.ManualTriggerAll();
                Inputs = _nodeFactory.Get(inputs);

                IsActive.OnChange += (b) => inputs.ManualTriggerAll();
            }
        }
    }
}
