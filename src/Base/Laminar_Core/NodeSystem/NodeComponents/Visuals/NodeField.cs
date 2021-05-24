using System;
using System.Collections.Generic;
using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Primitives.LaminarValue;
using Laminar_Core.Primitives.UserInterface;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Primitives;

namespace Laminar_Core.NodeSystem.NodeComponents.Visuals
{
    public class NodeField : VisualNodeComponent, INodeField
    {
        private readonly ILaminarValueStore _valueStore;
        private readonly IObjectFactory _factory;
        private object _displayedValueKey;

        public NodeField(Instance instance, IObservableValue<string> name, IFlow flowInput, IFlow flowOutput, IOpacity opacity, ILaminarValueStore valueStore, IUserInterfaceManager userInterfaces) 
            : base(name, flowInput, flowOutput, opacity)
        {
            _valueStore = valueStore;
            UserInterfaces = userInterfaces;

            UserInterfaces.Displays = instance.RegisteredDisplays;
            UserInterfaces.Editors = instance.RegisteredEditors;
            _factory = instance.Factory;

            _valueStore.ChildValueChanged += ValueStore_ChildValueChanged;
            _valueStore.ChangedAtKey += (o, e) => AnyValueChanged?.Invoke(this, e);
            Name.OnChange += _valueStore.SetValueName;
        }

        public event EventHandler<object> AnyValueChanged;

        public object this[object key] { get => _valueStore[key]; set => _valueStore[key] = value; }

        public IUserInterfaceManager UserInterfaces { get; }

        public ILaminarValue DisplayedValue { get; private set; }

        public object DisplayedValueKey
        {
            get => _displayedValueKey;
            set
            {
                _displayedValueKey = value;
                DisplayedValue = _valueStore.GetValue(value);
                UserInterfaces.SetChildValue(DisplayedValue);
                NotifyPropertyChanged(nameof(DisplayedValue));
            }
        }

        public IEnumerable<KeyValuePair<object, ILaminarValue>> AllValues => _valueStore.AllValues;

        public void AddValue(object key, object value, bool isUserEditable)
        {
            _valueStore.AddValue(key, value, isUserEditable);
            if (_valueStore.Count == 1)
            {
                DisplayedValueKey = key;
            }
        }

        public void AddValue<T>(object key, bool isUserEditable)
        {
            _valueStore.AddValue<T>(key, isUserEditable);
            if (_valueStore.Count == 1)
            {
                DisplayedValueKey = key;
            }
        }

        public void AddValue(object key, ILaminarValue value)
        {
            _valueStore.AddValue(key, value);
            if (_valueStore.Count == 1)
            {
                DisplayedValueKey = key;
            }
        }

        public ILaminarValue GetValue(object key) => _valueStore?.GetValue(key);

        public override INodeComponent Clone()
        {
            NodeField output = _factory.CreateInstance<NodeField>();
            CloneTo(output);
            return output;
        }

        public override void CloneTo(INodeComponent component)
        {
            if (component is not INodeField nodeField)
            {
                throw new ArgumentException("NodeField can only clone to other NodeFields");
            }

            base.CloneTo(component);
            foreach (var kvp in _valueStore.AllValues)
            {
                if (nodeField.GetValue(kvp.Key) is ILaminarValue existingValue)
                {
                    kvp.Value.CloneTo(existingValue);
                }
                else
                {
                    nodeField.AddValue(kvp.Key, (ILaminarValue)kvp.Value.Clone());
                }
            }
            nodeField.DisplayedValueKey = DisplayedValueKey;
        }

        private void ValueStore_ChildValueChanged(object sender, ILaminarValue laminarValue)
        {
            if (ParentNode is not null or IActionNode && laminarValue.IsUserEditable.Value)
            {
                INodeContainer.NodeBases[ParentNode].Update(null);
            }
            NotifyPropertyChanged("Child Value");
        }
    }
}
