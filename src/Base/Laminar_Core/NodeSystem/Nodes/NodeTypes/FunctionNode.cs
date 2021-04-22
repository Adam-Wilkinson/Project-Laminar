using Laminar_Core.NodeSystem.NodeComponents.Visuals;
using Laminar_Core.Scripts;
using Laminar_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.NodeSystem.Nodes.NodeTypes
{
    public class FunctionNode<T> : NodeContainer<T> where T : INode
    {
        bool _isLive = false;

        public FunctionNode(NodeDependencyAggregate dependencies) 
            : base(dependencies)
        {
        }

        public override void MakeLive()
        {
            _isLive = true;
        }

        public override void Update(IAdvancedScriptInstance instance)
        {
            if (!_isLive)
            {
                return;
            }

            _isLive = false;

            foreach (IVisualNodeComponentContainer component in (IList)Fields)
            {
                component.InputConnector.Activate(instance, Connection.PropagationDirection.Backwards);
            }

            try
            {
                TriggerEvaluate();
                ErrorState.Value = false;
            }
            catch
            {
                ErrorState.Value = true;
            }

            foreach (IVisualNodeComponentContainer component in (IList)Fields)
            {
                component.OutputConnector.Activate(instance, Connection.PropagationDirection.Forwards);
            }

            _isLive = true;
        }

        protected virtual void TriggerEvaluate()
        {
            (BaseNode as IFunctionNode).Evaluate();
        }
    }
}
