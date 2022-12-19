using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.NodeSystem.NodeTreeSystem;
using Laminar_Core.Scripting.Advanced.Compilation;
using Laminar_Core.Scripting.Advanced.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Scripting.Advanced.Instancing
{
    public interface IAdvancedScriptInstance : IScriptInstance
    {
        public INodeContainer Inputs { get; }

        public ICompiledScript CompiledScript { get; set; }
    }
}
