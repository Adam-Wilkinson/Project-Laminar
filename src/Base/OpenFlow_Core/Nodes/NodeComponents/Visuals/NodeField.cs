namespace OpenFlow_Core.Nodes.NodeComponents.Visuals
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using OpenFlow_Core.Primitives;
    using OpenFlow_Core.Primitives.LaminarValue;
    using OpenFlow_PluginFramework;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
    using OpenFlow_PluginFramework.NodeSystem.Nodes;
    using OpenFlow_PluginFramework.Primitives;

    public class NodeField : VisualNodeComponent, INodeField
    {
        private readonly ILaminarValueStore _valueStore;
        private object _displayedValueKey;

        public NodeField(IOpacity opacity, ILaminarValueStore valueStore) : base(opacity) 
        {
            _valueStore = valueStore;
            _valueStore.AnyValueChanged += ValueStore_AnyValueChanged;
        }

        public event EventHandler<object> ValueStoreChanged
        {
            add
            {
                _valueStore.ChangedAtKey += value;
            }
            remove
            {
                _valueStore.ChangedAtKey -= value;
            }
        }

        public override string Name
        {
            get => base.Name;
            set
            {
                base.Name = value;
                _valueStore.SetValueName(value);
            }
        }

        public object this[object key] { get => _valueStore[key]; set => _valueStore[key] = value; }

        public ILaminarValue DisplayedValue { get; private set; }

        public object DisplayedValueKey
        {
            get => _displayedValueKey;
            set
            {
                _displayedValueKey = value;
                DisplayedValue = _valueStore.GetValue(value);
            }
        }

        public void AddValue(object key, object value, bool isUserEditable)
        {
            _valueStore.AddValue(key, value, isUserEditable);
            if (_valueStore.Count == 1)
            {
                DisplayedValueKey = key;
            }
        }

        public ILaminarValue GetValue(object key) => _valueStore.GetValue(key);

        public override INodeComponent Clone() => CloneTo(new NodeField(Opacity.Clone(), _valueStore.Clone()) { DisplayedValueKey = _displayedValueKey });

        private void ValueStore_AnyValueChanged(object sender, EventArgs e)
        {
            ParentNode.TriggerEvaluate();
            NotifyPropertyChanged("Child Value");
        }
    }
}
