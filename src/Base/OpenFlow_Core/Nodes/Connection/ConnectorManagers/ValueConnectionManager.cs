using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public bool CheckIfConnectorExists(IVisualNodeComponent parentComponent, ConnectionType connectionType)
        {
            if (LaminarValue is not null)
            {
                LaminarValue.PropertyChanged -= LaminarValue_PropertyChanged;
                LaminarValue = null;
            }

            if (parentComponent is INodeField nodeField)
            {
                if (connectionType is ConnectionType.Input)
                {
                    LaminarValue = nodeField.GetValue(INodeField.InputKey);
                }

                if (connectionType is ConnectionType.Output)
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

        public void ConnectionAddedAction(IConnector connection)
        {
        }

        public void ConnectionRemovedAction(IConnector connection)
        {
        }

        public bool ConnectorExclusiveCheck(ConnectionType connectionType)
        {
            return connectionType is ConnectionType.Input;
        }

        public void HookupExistsCheck(IVisualNodeComponent component, ConnectionType connectionType)
        {
            if (component is INodeField field)
            {
                field.AnyValueChanged += (o, e) =>
                {
                    if (e as string is INodeField.InputKey && connectionType is ConnectionType.Input)
                    {
                        ExistsChanged?.Invoke(this, new EventArgs());
                    }

                    if (e as string is INodeField.OutputKey && connectionType is ConnectionType.Output)
                    {
                        ExistsChanged?.Invoke(this, new EventArgs());
                    }
                };
            }
        }
    }
}
