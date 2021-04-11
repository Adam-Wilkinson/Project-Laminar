using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Collections;
using OpenFlow_PluginFramework.Primitives;
using System;
using System.Collections.Generic;

namespace OpenFlow_Core.NodeSystem.NodeComponents.Collections
{
    public class NodeComponentList : NodeComponentCollection, INodeComponentList
    {
        public NodeComponentList(IOpacity opacity) : base(opacity) { }

        public INodeComponent this[int index] { get => base[index]; set => base[index] = value; }

        public bool IsReadOnly => false;

        public int Count => ComponentCount;

        public void Clear() => ProtectedReset();

        public void CopyTo(INodeComponent[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public void Add(INodeComponent item) => ProtectedAdd(item);

        public void Insert(int index, INodeComponent item) => ProtectedInsert(index, item);

        public bool Remove(INodeComponent item) => ProtectedRemove(item);

        public void RemoveAt(int index) => ProtectedRemoveAt(index);

        public void AddRange(IEnumerable<INodeComponent> elements)
        {
            foreach (INodeComponent component in elements)
            {
                Add(component);
            }
        }
    }
}
