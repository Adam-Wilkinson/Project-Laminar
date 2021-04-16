using Laminar_PluginFramework.NodeSystem.Nodes;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Laminar_PluginFramework.NodeSystem.NodeComponents.Collections
{
    public interface INodeComponentCollection : INotifyCollectionChanged, INodeComponent, IEnumerable<INodeComponent>
    {
        public INotifyCollectionChanged VisualNodeComponentsObservable { get; }

        bool Contains(INodeComponent component);

        int IndexOf(INodeComponent component);
    }
}