namespace OpenFlow_Core.Nodes.NodeComponents.Visuals
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using OpenFlow_Core.Nodes.Connectors;
    using OpenFlow_Core.Primitives;
    using OpenFlow_Core.Primitives.LaminarValue;
    using OpenFlow_Core.Primitives.UserInterface;
    using OpenFlow_PluginFramework;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
    using OpenFlow_PluginFramework.NodeSystem.Nodes;
    using OpenFlow_PluginFramework.Primitives;

    public class NodeField : DisplayableNodeComponent, INodeField
    {
        private readonly ILaminarValueStore _valueStore;
        private object _displayedValueKey;

        public NodeField(IObservableValue<string> name, IOpacity opacity, IConnectionManager connectionManager, ILaminarValueStore valueStore, IUserInterfaceManager userInterfaces) 
            : base(name, opacity, connectionManager) 
        {
            _valueStore = valueStore;
            UserInterfaces = userInterfaces;
            _valueStore.AnyValueChanged += ValueStore_AnyValueChanged;
            _valueStore.ChangedAtKey += BaseField_ValueStoreChanged;

            ConnectionManager.AddConnectionCheck((connectionType) => ValueConnector.CheckConnector(this, connectionType));
            Name.OnChange(_valueStore.SetValueName);

        }

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

        private void BaseField_ValueStoreChanged(object sender, object e)
        {
            if (e as string is INodeField.InputKey)
            {
                ConnectionManager.UpdateInput();
            }
            else if (e as string is INodeField.OutputKey)
            {
                ConnectionManager.UpdateOutput();
            }
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
