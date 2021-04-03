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

    public class NodeField : VisualNodeComponent, INodeField
    {
        private readonly ILaminarValueStore _valueStore;
        private object _displayedValueKey;

        public NodeField(IOpacity opacity, ILaminarValueStore valueStore, IUserInterfaceManager userInterfaces) : base(opacity) 
        {
            _valueStore = valueStore;
            UserInterfaces = userInterfaces;
            _valueStore.AnyValueChanged += ValueStore_AnyValueChanged;
            _valueStore.ChangedAtKey += BaseField_ValueStoreChanged;

            UpdateInput();
            UpdateOutput();
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
                UpdateDisplayedValue();
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

        public override INodeComponent Clone() => CloneTo(new NodeField(Opacity.Clone(), _valueStore.Clone(), UserInterfaces.Clone()) { DisplayedValueKey = DisplayedValueKey });

        public IUserInterfaceManager UserInterfaces { get; }

        protected override bool TryUpdateConnector(IConnector connector, ConnectionType connectionType, out IConnector newConnector)
        {
            if (base.TryUpdateConnector(connector, connectionType, out newConnector))
            {
                return true;
            }

            ILaminarValue relevantValue = connectionType == ConnectionType.Input ? GetValue(INodeField.InputKey) : GetValue(INodeField.OutputKey);
            if (relevantValue != null && connector is not ValueConnector)
            {
                newConnector = new ValueConnector(relevantValue, ParentNodeBase, connectionType);
                return true;
            }

            newConnector = default;
            return false;
        }

        protected override HorizontalAlignment CalculateAlignment()
        {
            return
                InputConnector.Value is null && OutputConnector.Value is not null ? HorizontalAlignment.Right : (
                InputConnector.Value is not null && OutputConnector.Value is null ? HorizontalAlignment.Left :
                HorizontalAlignment.Middle);
        }

        private void BaseField_ValueStoreChanged(object sender, object e)
        {
            if (e as string is INodeField.InputKey)
            {
                UpdateInput();
            }
            else if (e as string is INodeField.OutputKey)
            {
                UpdateOutput();
            }
        }

        private void UpdateDisplayedValue()
        {
            UserInterfaces.SetChildValue(DisplayedValue);
            DisplayedValue.Name = Name;
            NotifyPropertyChanged(nameof(DisplayedValue));
        }

        private void ValueStore_AnyValueChanged(object sender, EventArgs e)
        {
            ParentNode.TriggerEvaluate();
            NotifyPropertyChanged("Child Value");
        }
    }
}
