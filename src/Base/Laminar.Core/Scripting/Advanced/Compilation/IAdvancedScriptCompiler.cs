using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Scripting.Advanced.Editing;
using Laminar_Core.Scripting.Advanced.Instancing;
using Laminar_PluginFramework.Primitives;
using System.Collections.Generic;

namespace Laminar_Core.Scripting.Advanced.Compilation
{
    public interface IAdvancedScriptCompiler
    {
        public ICompiledScript Compile(IAdvancedScript script);

        public Dictionary<InputNode, ILaminarValue> Inputs { get; }

        public Dictionary<INodeContainer, CompiledNodeWrapper> AllNodes { get; }
    }
}
