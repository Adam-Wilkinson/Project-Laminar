using OpenFlow_Core.Nodes.VisualNodeComponentDisplays;
using OpenFlow_Core.Nodes.NodeComponents.Visuals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;

namespace OpenFlow_Core.Nodes
{
    class VisualNodeComponentMapper : ITypeMapper<IVisualNodeComponent, IVisualNodeComponentDisplay>
    {
        private readonly NodeBase _parentNode;

        public VisualNodeComponentMapper(NodeBase parentNode)
        {
            _parentNode = parentNode;
        }

        public IVisualNodeComponentDisplay MapType(IVisualNodeComponent toMap)
        {
            if (typeof(INodeField).IsAssignableFrom(toMap.GetType()))
            {
                return new NodeFieldDisplay((INodeField)toMap, _parentNode);
            }

            if (typeof(INodeLabel).IsAssignableFrom(toMap.GetType()))
            {
                return new NodeLabelDisplay(_parentNode, (INodeLabel)toMap);
            }

            if (toMap is INodeDecorator decorator)
            {
                switch (decorator.DecoratorType)
                {
                    case NodeDecoratorType.MajorSeparator:
                        return new MajorSeperator(_parentNode, decorator);
                    case NodeDecoratorType.MinorSeparator:
                        return new MinorSeperator(_parentNode, decorator);
                }
            }

            return null;
        }
    }
}
