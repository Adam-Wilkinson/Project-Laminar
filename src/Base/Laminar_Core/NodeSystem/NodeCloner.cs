using Laminar_Core.NodeSystem.Nodes;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.NodeSystem
{
    public static class NodeCloner
    {
        /*
        private readonly INodeFactory _nodeFactory;

        public NodeCloner(INodeFactory nodeFactory)
        {
            _nodeFactory = nodeFactory;
        }
        */

        //public static T Clone<T>(this T toClone) where T : INode, new()
        //{
        //    T newNode = new();

        //    foreach ((INodeComponent, INodeComponent) componentPair in toClone.Fields.Zip(newNode.Fields))
        //    {
        //        componentPair.Item1.CloneTo(componentPair.Item2);
        //    }

        //    return toClone;
        //}
    }
}
