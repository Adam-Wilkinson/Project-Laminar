using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.NodeSystem.NodeTreeSystem;
using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Scripts
{
    public class AdvancedScriptInstance : Script, IAdvancedScriptInstance
    {
        private readonly INodeFactory _nodeFactory;
        private INodeTree _script;

        public AdvancedScriptInstance(ScriptDependencyAggregate deps, INodeFactory nodeFactory) : base(deps)
        {
            IsActive.Value = false;
            _nodeFactory = nodeFactory;
        }

        public INodeContainer Inputs { get; private set; }

        public INodeTree Script
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
