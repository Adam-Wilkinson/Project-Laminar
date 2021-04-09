using OpenFlow_Core.Nodes.NodeComponents.Visuals;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core.Primitives.ObservableCollectionMapper
{
    public class VisualNodeComponentContainerMapper : ITypeMapper<IVisualNodeComponent, IVisualNodeComponentContainer>
    {
        public IVisualNodeComponentContainer MapType(IVisualNodeComponent input)
        {
            IVisualNodeComponentContainer output = Instance.Factory.GetImplementation<IVisualNodeComponentContainer>();

            output.Child = input;

            return output;
        }
    }
}
