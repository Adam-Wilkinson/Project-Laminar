using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Collections;
using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Laminar_Core.NodeSystem.NodeComponents.Collections
{
    public class NodeComponentDictionary : NodeComponentCollection, IDictionary<object, INodeComponent>, INodeComponentDictionary
    {
        private readonly Dictionary<object, INodeComponent> _subComponents = new();

        public NodeComponentDictionary(IOpacity opacity) : base(opacity) { }

        public ICollection<object> Keys => _subComponents.Keys;

        public ICollection<INodeComponent> Values => _subComponents.Values;

        public int Count => _subComponents.Count;

        public bool IsReadOnly => false;

        public INodeComponent this[object key] { get => _subComponents[key]; set => _subComponents[key] = value; }

        public bool ShowSectionByKey(object key)
        {
            if (_subComponents.TryGetValue(key, out INodeComponent component) && !Contains(component))
            {
                ProtectedAdd(component);
                return true;
            }

            return false;
        }

        public bool HideComponentByKey(object key)
        {
            if (_subComponents.TryGetValue(key, out INodeComponent component) && Contains(component))
            {
                ProtectedRemove(component);
                return true;
            }

            return false;
        }

        public void HideAllComponents()
        {
            ProtectedReset();
        }

        public void Add(object key, INodeComponent value) => _subComponents.Add(key, value);

        public bool ContainsKey(object key) => _subComponents.ContainsKey(key);

        public bool Remove(object key) => _subComponents.Remove(key);

        public bool TryGetValue(object key, [MaybeNullWhen(false)] out INodeComponent value) => _subComponents.TryGetValue(key, out value);

        public void Add(KeyValuePair<object, INodeComponent> item) => _subComponents.Add(item.Key, item.Value);

        public void Clear() => _subComponents.Clear();

        public bool Contains(KeyValuePair<object, INodeComponent> item) => _subComponents.ContainsKey(item.Key) && _subComponents[item.Key] == item.Value;

        public void CopyTo(KeyValuePair<object, INodeComponent>[] array, int arrayIndex) => throw new NotImplementedException();

        public bool Remove(KeyValuePair<object, INodeComponent> item) => _subComponents.Remove(item.Key);

        IEnumerator<KeyValuePair<object, INodeComponent>> IEnumerable<KeyValuePair<object, INodeComponent>>.GetEnumerator() => _subComponents.GetEnumerator();
    }
}
