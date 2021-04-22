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
        private INodeTree _script;

        public AdvancedScriptInstance(ScriptDependencyAggregate deps) : base(deps)
        {
        }

        public INodeContainer Inputs { get; private set; }

        public INodeTree Script
        {
            get => _script;
            set => _script = value;
        }
    }
}
