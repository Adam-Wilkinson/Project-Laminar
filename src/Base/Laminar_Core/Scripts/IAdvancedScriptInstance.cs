using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.NodeSystem.NodeTreeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Scripts
{
    public interface IAdvancedScriptInstance : IScriptInstance
    {
        public INodeContainer Inputs { get; }

        public INodeTree Script { get; set; }
    }
}
