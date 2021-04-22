using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Scripts;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.Primitives;
using Laminar_PluginFramework.Primitives.TypeDefinition;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Laminar_Core.NodeSystem.Connection.ConnectorManagers
{
    public class ValueConnectionManager : IConnectorManager
    {
        private IVisualNodeComponent _parentComponent;
        private ConnectorType _connectorType;
        private ValueConnectionManager _pairedManager;
        private readonly Instance _instance;
        private bool _isActivating;
        private object _inputConnectorOldValue;
        private ILaminarValue _inputLaminarValue;

        public ValueConnectionManager(Instance instance, IObservableValue<string> hexColour)
        {
            _instance = instance;
            HexColour = hexColour;
        }

        public IObservableValue<string> HexColour { get; }

        public ILaminarValue LaminarValue { get; private set; }

        public event EventHandler ExistsChanged;

        public void Initialize(IVisualNodeComponent component, ConnectorType connectionType)
        {
            _parentComponent = component;
            _connectorType = connectionType;

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

        public bool ConnectorExists()
        {
            if (LaminarValue is not null)
            {
                LaminarValue.TypeDefinitionChanged -= LaminarValue_TypeDefinitionChanged;
                LaminarValue = null;
            }

            if (_parentComponent is INodeField nodeField)
            {
                if (_connectorType is ConnectorType.Input)
                {
                    LaminarValue = nodeField.GetValue(INodeField.InputKey);
                }

                if (_connectorType is ConnectorType.Output)
                {
                    LaminarValue = nodeField.GetValue(INodeField.OutputKey);
                }

                if (LaminarValue is not null)
                {
                    HexColour.Value = LaminarValue.TypeDefinition is not null ? _instance.GetTypeInfo(LaminarValue.TypeDefinition.ValueType).HexColour : "#FFFFFF";
                    LaminarValue.TypeDefinitionChanged += LaminarValue_TypeDefinitionChanged;
                    return true;
                }
            }

            return false;
        }

        private void LaminarValue_TypeDefinitionChanged(object sender, ITypeDefinition e)
        {
            HexColour.Value = e is not null ? _instance.GetTypeInfo(e.ValueType).HexColour : "#FFFFFF";
        }

        public bool CompatibilityCheck(IConnectorManager toCheck)
        {
            return toCheck is ValueConnectionManager valConnection && valConnection.LaminarValue.CanSetValue(LaminarValue.Value);
        }

        public void ConnectionAddedAction(IConnectorManager manager)
        {
            _pairedManager = manager as ValueConnectionManager;
            if (_connectorType is ConnectorType.Output && manager is ValueConnectionManager valConnection)
            {
                _inputLaminarValue = valConnection.LaminarValue;
                _inputConnectorOldValue = _inputLaminarValue.Value;
                _inputLaminarValue.Value = LaminarValue.Value;
                _inputLaminarValue.IsUserEditable.Value = false;
            }
        }

        public void ConnectionRemovedAction(IConnectorManager manager)
        {
            _pairedManager = null;
            if (_connectorType is ConnectorType.Output && manager is ValueConnectionManager valConnection)
            {
                valConnection.LaminarValue.IsUserEditable.Value = true;
                valConnection.LaminarValue.Value = _inputConnectorOldValue;
            }
        }

        public bool ConnectorExclusiveCheck()
        {
            return _connectorType is ConnectorType.Input;
        }

        public void Activate(IAdvancedScriptInstance instance, PropagationDirection direction)
        {
            if (_isActivating || _pairedManager is null)
            {
                return;
            }

            _isActivating = true;

            INodeContainer parentNode = INodeContainer.NodeBases[_parentComponent.ParentNode];

            if (_connectorType is ConnectorType.Input)
            {
                if (direction is PropagationDirection.Forwards)
                {
                    parentNode.Update(instance);
                }

                if (direction is PropagationDirection.Backwards)
                {
                    _pairedManager?.Activate(instance, direction);
                }
            }

            if (_connectorType is ConnectorType.Output)
            {
                if (direction is PropagationDirection.Forwards)
                {
                    _inputLaminarValue.Value = LaminarValue.Value;
                    _pairedManager?.Activate(instance, direction);
                }
                if (direction is PropagationDirection.Backwards)
                {
                    parentNode.Update(instance);
                }
            }

            _isActivating = false;
        }
    }
}
