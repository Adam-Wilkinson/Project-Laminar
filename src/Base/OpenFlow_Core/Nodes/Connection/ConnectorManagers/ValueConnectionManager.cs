using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.Primitives;
using System;
using System.ComponentModel;

namespace OpenFlow_Core.Nodes.Connection.ConnectorManagers
{
    public class ValueConnectionManager : IConnectorManager
    {
        public ValueConnectionManager(IObservableValue<string> hexColour)
        {
            HexColour = hexColour;
        }

        public IObservableValue<string> HexColour { get; }

        public ILaminarValue LaminarValue { get; private set; }

        public event EventHandler ExistsChanged;

        public bool CheckIfConnectorExists(IVisualNodeComponent parentComponent, ConnectorType connectionType)
        {
            if (LaminarValue is not null)
            {
                LaminarValue.PropertyChanged -= LaminarValue_PropertyChanged;
                LaminarValue = null;
            }

            if (parentComponent is INodeField nodeField)
            {
                if (connectionType is ConnectorType.Input)
                {
                    LaminarValue = nodeField.GetValue(INodeField.InputKey);
                }

                if (connectionType is ConnectorType.Output)
                {
                    LaminarValue = nodeField.GetValue(INodeField.OutputKey);
                }

                if (LaminarValue is not null)
                {
                    HexColour.Value = LaminarValue.TypeDefinition is not null ? Instance.Current.GetTypeInfo(LaminarValue.TypeDefinition.ValueType).HexColour : "#FFFFFF";
                    LaminarValue.PropertyChanged += LaminarValue_PropertyChanged;
                    return true;
                }
            }

            return false;
        }

        private void LaminarValue_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(ILaminarValue.TypeDefinition))
            {
                HexColour.Value = LaminarValue.TypeDefinition is not null ? Instance.Current.GetTypeInfo(LaminarValue.TypeDefinition.ValueType).HexColour : "#FFFFFF";
            }
        }

        public bool CompatibilityCheck(IConnectorManager toCheck)
        {
            return toCheck is ValueConnectionManager valConnection && valConnection.LaminarValue.CanSetValue(LaminarValue.Value);
        }

        public void ConnectionAddedAction(IConnectorManager manager, ConnectorType myConnectorType)
        {
            if (myConnectorType is ConnectorType.Output && manager is ValueConnectionManager valConnection)
            {
                valConnection.LaminarValue.Driver = LaminarValue;
            }
        }

        public void ConnectionRemovedAction(IConnectorManager manager, ConnectorType myConnectorType)
        {
            if (myConnectorType is ConnectorType.Output && manager is ValueConnectionManager valConnection && valConnection.LaminarValue.Driver == LaminarValue)
            {
                valConnection.LaminarValue.Driver = null;
            }
        }

        public bool ConnectorExclusiveCheck(ConnectorType connectionType)
        {
            return connectionType is ConnectorType.Input;
        }

        public void HookupExistsCheck(IVisualNodeComponent component, ConnectorType connectionType)
        {
            if (component is INodeField field)
            {
                field.AnyValueChanged += (o, e) =>
                {
                    if (e as string is INodeField.InputKey && connectionType is ConnectorType.Input)
                    {
                        ExistsChanged?.Invoke(this, new EventArgs());
                    }

                    if (e as string is INodeField.OutputKey && connectionType is ConnectorType.Output)
                    {
                        ExistsChanged?.Invoke(this, new EventArgs());
                    }
                };
            }
        }
    }
}
