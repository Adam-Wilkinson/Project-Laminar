using System.Collections.Generic;

namespace OpenFlow_PluginFramework.NodeSystem.NodeComponents.Collections
{
    public interface INodeComponentList : INodeComponentCollection, IList<INodeComponent>
    {
    }
}