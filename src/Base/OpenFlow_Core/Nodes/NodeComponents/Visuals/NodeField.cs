namespace OpenFlow_Core.Nodes.NodeComponents.Visuals
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using OpenFlow_PluginFramework;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
    using OpenFlow_PluginFramework.NodeSystem.Nodes;
    using OpenFlow_PluginFramework.Primitives;
    using OpenFlow_PluginFramework.Primitives.TypeDefinition;

    public class NodeField : VisualNodeComponent, INodeField
    {
        private readonly Dictionary<object, ILaminarValue> _valueStore = new();

        public NodeField(IOpacity opacity) : base(opacity) { }

        public event EventHandler<object> ValueStoreChanged;

        public event EventHandler<object> AnyValueChanged;

        public override string Name
        {
            get => base.Name;
            set
            {
                base.Name = value;
                foreach (ILaminarValue storedValue in _valueStore.Values)
                {
                    storedValue.Name = base.Name;
                }
            }
        }

        public object Input
        {
            get => GetDisplayValue(INodeField.InputKey)?.Value;
            set => this[INodeField.InputKey] = value;
        }

        public object Output
        {
            get => GetDisplayValue(INodeField.OutputKey)?.Value;
            set => this[INodeField.OutputKey] = value;
        }

        public object this[object key]
        {
            get => GetDisplayValue(key)?.Value;
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                if (value == null)
                {
                    RemoveValue(key);
                }
                else if (_valueStore.TryGetValue(key, out ILaminarValue OFVal))
                {
                    if (OFVal.CanSetValue(value))
                    {
                        OFVal.Value = value;
                    }
                    else
                    {
                        RemoveValue(key);
                        AddValue(key, Constructor.LaminarValue(Constructor.RigidTypeDefinitionManager(value), true));
                    }
                }
                else
                {
                    AddValue(key, Constructor.LaminarValue(Constructor.RigidTypeDefinitionManager(value), true));
                }
            }
        }

        public ILaminarValue DisplayedValue { get; private set; }

        public void AddValue(object key, ILaminarValue newVal)
        {
            _valueStore.Add(key, newVal);
            newVal.PropertyChanged += ChildValue_PropertyChanged;
            if (_valueStore.Count == 1)
            {
                SetDisplayedValue(key);
            }
            ValueStoreChanged?.Invoke(this, key);
        }

        public ILaminarValue GetDisplayValue(object key) => key != null && _valueStore.TryGetValue(key, out ILaminarValue value) ? value : null;

        public override INodeComponent Clone() => CloneTo(Constructor.NodeField(Name));

        protected override INodeField CloneTo(INodeComponent nodeField)
        {
            base.CloneTo(nodeField);
            foreach (KeyValuePair<object, ILaminarValue> kvp in _valueStore)
            {
                (nodeField as NodeField).AddValue(kvp.Key, kvp.Value.Clone());
            }

            return (nodeField as NodeField);
        }

        private void SetDisplayedValue(object displayValueKey)
        {
            DisplayedValue = GetDisplayValue(displayValueKey);
            NotifyPropertyChanged(nameof(DisplayedValue));
        }

        private bool RemoveValue(object key)
        {
            if (_valueStore.TryGetValue(key, out ILaminarValue val))
            {
                val.PropertyChanged -= ChildValue_PropertyChanged;
                _valueStore.Remove(key);
                ValueStoreChanged?.Invoke(this, key);
                return true;
            }
            return false;
        }

        private void ChildValue_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ILaminarValue.Value))
            {
                ParentNode.TriggerEvaluate();
                AnyValueChanged?.Invoke(this, (sender as ILaminarValue)?.Value);
                NotifyPropertyChanged("Child Value");
            }
        }
    }
}
