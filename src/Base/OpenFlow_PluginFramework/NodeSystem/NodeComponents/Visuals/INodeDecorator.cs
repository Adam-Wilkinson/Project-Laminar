using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals
{
    public interface INodeDecorator : IVisualNodeComponent
    {
        public NodeDecoratorType DecoratorType { get; set; }
    }
}
