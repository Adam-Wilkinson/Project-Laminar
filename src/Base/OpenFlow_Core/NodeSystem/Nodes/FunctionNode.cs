using OpenFlow_Core.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core.NodeSystem.Nodes
{
    public class FunctionNode<T> : NodeBase<T> where T : INode
    {
        bool IsLive = false;

        public FunctionNode(NodeDependencyAggregate dependencies) 
            : base(dependencies)
        {
        }

        public override void MakeLive()
        {
            IsLive = true;
        }

        public override void Update()
        {
            if (!IsLive)
            {
                return;
            }

            IsLive = false;

            foreach (IVisualNodeComponentContainer component in (IList)Fields)
            {
                component.InputConnector.Activate();
            }

            try
            {
                (BaseNode as IFunctionNode).Evaluate();
                ErrorState.Value = false;
            }
            catch
            {
                ErrorState.Value = true;
            }

            foreach (IVisualNodeComponentContainer component in (IList)Fields)
            {
                component.OutputConnector.Activate();
            }

            IsLive = true;
        }
    }
}
