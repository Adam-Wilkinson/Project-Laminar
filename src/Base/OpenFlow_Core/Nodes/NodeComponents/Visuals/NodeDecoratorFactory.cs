using OpenFlow_Core.Primitives;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core.Nodes.NodeComponents.Visuals
{
    public class NodeDecoratorFactory : INodeDecoratorFactory
    {
        private readonly IOpacity _opacity;

        public NodeDecoratorFactory(IOpacity opacityClass)
        {
            _opacity = opacityClass;
        }

        public INodeDecorator GetDecorator(NodeDecoratorType type) => (type) switch
        {
            NodeDecoratorType.MajorSeparator => new MajorSeperator(_opacity.Clone()),
            NodeDecoratorType.MinorSeparator => new MinorSeperator(_opacity.Clone()),
            _ => throw new NotImplementedException(),
        };
    }
}
