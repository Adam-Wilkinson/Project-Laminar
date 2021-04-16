using System;
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
        private object _displayedValueKey;

        public NodeField(IObservableValue<string> name, IOpacity opacity, ILaminarValueStore valueStore, IUserInterfaceManager userInterfaces) 
            : base(name, opacity)
        {
            _valueStore = valueStore;
            UserInterfaces = userInterfaces;
            _valueStore.AnyValueChanged += ValueStore_AnyValueChanged;
            _valueStore.ChangedAtKey += (o, e) => AnyValueChanged?.Invoke(this, e);
            Name.OnChange(_valueStore.SetValueName);
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

        public ILaminarValue GetValue(object key) => _valueStore?.GetValue(key);

        public void SetValueStore(ILaminarValueStore copyFrom)
        {
            _valueStore.CopyFrom(copyFrom);
        }

        public override INodeComponent Clone()
        {
            NodeField output = Instance.Factory.GetImplementation<INodeField>() as NodeField;
            CloneTo(output);
            output.SetValueStore(_valueStore);
            output.DisplayedValueKey = DisplayedValueKey;
            return output;
        }

        private void ValueStore_AnyValueChanged(object sender, EventArgs e)
        {
            if (ParentNode is not IActionNode)
            {
                INodeBase.NodeBases[ParentNode].Update();
            }
            NotifyPropertyChanged("Child Value");
        }
    }
}
