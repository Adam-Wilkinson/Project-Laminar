using System.Collections.Generic;

namespace Laminar_PluginFramework.NodeSystem.NodeComponents.Collections
{
    public interface INodeComponentList : INodeComponentCollection, IList<INodeComponent>
    {
        public void AddRange(IEnumerable<INodeComponent> elements);

        public void RemoveAll();
    }
}