using Laminar_Core.NodeSystem.NodeComponents.Visuals;
using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Scripting.Advanced.Editing.Connection;
using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Scripting.Advanced.Compilation
{
    public class CompiledNodeChain : IDisposable
    {
        private readonly IFlow trigger;
        private readonly List<CompiledNodeWrapper> nodesInChain = new();

        public CompiledNodeChain(IVisualNodeComponentContainer root, IFlow triggerFlow, IAdvancedScriptCompiler compilerInstance)
        {
            trigger = triggerFlow;
            trigger.Activated += StartActivation;

            while (root.OutputConnector.ExclusiveConnection?.InputConnector is IConnector nextNodeFlowInput)
            {
                nodesInChain.Add(new(nextNodeFlowInput.ConnectorNode, compilerInstance));
                root = nextNodeFlowInput.ConnectorNode.Name;
            }
        }

        public void StartActivation(object sender, EventArgs e)
        {
            foreach (CompiledNodeWrapper node in nodesInChain)
            {
                node.Activate();
            }
        }

        public void Dispose()
        {
            trigger.Activated -= StartActivation;
            foreach (CompiledNodeWrapper node in nodesInChain)
            {
                node.Dispose();
            }
        }
    }
}
