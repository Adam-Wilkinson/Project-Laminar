using System;
using OpenFlow_Core.Primitives.LaminarValue;
using OpenFlow_Core.Primitives.UserInterface;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.Primitives;

namespace OpenFlow_Core.Nodes.NodeComponents.Visuals
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
            if (ParentNode != null)
            {
                INodeBase.NodeBases[ParentNode].TryEvaluate();
            }
            NotifyPropertyChanged("Child Value");
        }
    }
}
