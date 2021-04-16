using Laminar_Core.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Primitives.ObservableCollectionMapper
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
