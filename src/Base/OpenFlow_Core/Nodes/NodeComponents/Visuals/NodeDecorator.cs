using OpenFlow_PluginFramework;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core.Nodes.NodeComponents.Visuals
{
    class NodeDecorator : VisualNodeComponent, INodeDecorator
    {
        public NodeDecorator(IOpacity opacity) : base(opacity) { }

        public NodeDecoratorType DecoratorType { get; set; }

        public override INodeComponent Clone() => CloneTo(Constructor.NodeDecorator(DecoratorType));
    }
}
