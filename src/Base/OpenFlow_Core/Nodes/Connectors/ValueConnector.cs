namespace OpenFlow_Core.Nodes.Connectors
{
    using System.ComponentModel;
    using OpenFlow_PluginFramework.NodeSystem;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
    using OpenFlow_PluginFramework.Primitives;

    public class ValueConnector : Connector<ValueConnector>, INotifyPropertyChanged
    {
        public ValueConnector(ILaminarValue displayValue, NodeBase parent, ConnectionType connectionType)
            : base(parent, connectionType)
        {
            DisplayValue = displayValue;
            DisplayValue.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == nameof(DisplayValue.TypeDefinition))
                {
                    PropertyChanged?.Invoke(o, new PropertyChangedEventArgs(nameof(ColourHex)));
                }
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ILaminarValue DisplayValue { get; }

        public override bool IsExclusiveConnection => ConnectionType == ConnectionType.Input;

        public override string ColourHex => DisplayValue.TypeDefinition != null ? Instance.Current.GetTypeInfo(DisplayValue.TypeDefinition.ValueType).HexColour : "#FFFFFF";

        protected override bool CanAddConnection(ValueConnector connector) => base.CanAddConnection(connector) && DisplayValue.CanSetValue(connector.DisplayValue.Value);

        protected override void ConnectorAdded(ValueConnector e)
        {
            if (ConnectionType == ConnectionType.Input)
            {
                DisplayValue.Driver = e.DisplayValue;
                ParentNode?.TryEvaluate();
            }
        }

        protected override void ConnectorRemoved(ValueConnector e)
        {
            if (ConnectionType == ConnectionType.Input)
            {
                DisplayValue.Driver = null;
                ParentNode?.TryEvaluate();
            }
        }

        public static ValueConnector CheckConnector(INodeField field, ConnectionType connectionType)
        {
            ILaminarValue relevantValue = connectionType == ConnectionType.Input ? field.GetValue(INodeField.InputKey) : field.GetValue(INodeField.OutputKey);
            if (relevantValue != null)
            {
                return new ValueConnector(relevantValue, null, connectionType);
            }

            return null;
        }
    }
}
