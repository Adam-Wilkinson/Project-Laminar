using Laminar_Core.NodeSystem.NodeComponents.Visuals;
using Laminar_Core.Scripting;
using Laminar_Core.Scripting.Advanced.Instancing;
using Laminar_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.NodeSystem.Nodes.NodeTypes
{
    public class FunctionNode<T> : NodeContainer<T> where T : INode, new()
    {
        public FunctionNode(NodeDependencyAggregate dependencies) 
            : base(dependencies)
        {
        }

        protected override void SafeUpdate(IAdvancedScriptInstance instance)
        {
            try
            {
                (BaseNode as IFunctionNode).Evaluate();
                ErrorState.Value = false;
            }
            catch
            {
                ErrorState.Value = true;
            }
        }
    }
}
