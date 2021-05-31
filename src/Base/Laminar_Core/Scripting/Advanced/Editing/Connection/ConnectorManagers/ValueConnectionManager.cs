using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Scripting;
using Laminar_Core.Scripting.Advanced.Editing.Connection.ConnectorManagers;
using Laminar_Core.Scripting.Advanced.Instancing;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.Primitives;
using Laminar_PluginFramework.Primitives.TypeDefinition;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace Laminar_Core.Scripting.Advanced.Editing.Connection.ConnectorManagers
{
    public class ValueConnectionManager : IConnectorManager
    {
        private readonly List<ValueConnectionManager> _pairedManagers = new();
        private readonly Instance _instance;
        private IVisualNodeComponent _parentComponent;
        private ConnectorType _connectorType;
        private bool _isActivating;
        private object _oldValue;

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
            if (manager is not ValueConnectionManager valConnection)
            {
                throw new ArgumentException("ValueConnectionManager can only connect to other ValueConnectionManagers");
            }

            _pairedManagers.Add(valConnection);

            if (_connectorType is ConnectorType.Input)
            {
                _oldValue = LaminarValue.Value;
                LaminarValue.IsUserEditable.Value = false;
                manager.Activate(null, PropagationDirection.Forwards);
            }
        }

        public void ConnectionRemovedAction(IConnectorManager manager)
        {
            if (manager is not ValueConnectionManager valConnection)
            {
                throw new ArgumentException("ValueConnectionManager can only connect to other ValueConnectionManagers");
            }

            _pairedManagers.Remove(valConnection);
            if (_connectorType is ConnectorType.Input)
            {
                LaminarValue.IsUserEditable.Value = true;
                LaminarValue.Value = _oldValue;
            }
        }

        public bool ConnectorExclusiveCheck()
        {
            return _connectorType is ConnectorType.Input;
        }

        public void Activate(IAdvancedScriptInstance instance, PropagationDirection direction)
        {
            if (_isActivating || _pairedManagers.Count is 0)
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
                    foreach (ValueConnectionManager manager in _pairedManagers)
                    {
                        manager.Activate(instance, direction);
                    }
                }
            }

            if (_connectorType is ConnectorType.Output)
            {
                if (direction is PropagationDirection.Forwards)
                {
                    foreach (ValueConnectionManager manager in _pairedManagers)
                    {
                        (manager._parentComponent as INodeField).SetInput((_parentComponent as INodeField).GetOutput());
                        // INodeContainer.NodeBases[manager._parentComponent.ParentNode].SetFieldValue(instance, manager._parentComponent as INodeField, _inputLaminarValue, parentNode.GetFieldValue(instance, _parentComponent as INodeField, LaminarValue));
                        manager.Activate(instance, direction);
                    }
                }
                if (direction is PropagationDirection.Backwards)
                {
                    foreach (ValueConnectionManager manager in _pairedManagers)
                    {
                        parentNode.Update(instance);
                        (manager._parentComponent as INodeField).SetInput((_parentComponent as INodeField).GetOutput());
                        // INodeContainer.NodeBases[manager._parentComponent.ParentNode].SetFieldValue(instance, manager._parentComponent as INodeField, _inputLaminarValue, parentNode.GetFieldValue(instance, _parentComponent as INodeField, LaminarValue));
                    }
                }
            }

            _isActivating = false;
        }
    }
}
