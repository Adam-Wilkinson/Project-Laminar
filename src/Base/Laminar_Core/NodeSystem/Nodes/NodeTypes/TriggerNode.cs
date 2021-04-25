using Laminar_Core.Scripts;
using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Collections;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;

namespace Laminar_Core.NodeSystem.Nodes.NodeTypes
{
    public class TriggerNode<T> : NodeContainer<T> where T : INode, new()
    {
        private readonly Dictionary<IAdvancedScriptInstance, InstanceManager> _instanceManagers = new();

        public TriggerNode(NodeDependencyAggregate dependencies) : base(dependencies)
        {
            NameLabel.SetFlowOutput(true);
            FlowOutContainer = Name;
        }

        ~TriggerNode()
        {
            (BaseNode as ITriggerNode).RemoveTriggers();
            foreach (var kvp in _instanceManagers)
            {
                kvp.Value.Dispose();
            }
        }

        public override T BaseNode
        {
            set
            {
                base.BaseNode = value;
                (BaseNode as ITriggerNode).Trigger += TriggerNode_Trigger;
            }
        }

        public override bool IsLive 
        {
            set
            {
                base.IsLive = value;
                if (IsLive)
                {
                    (BaseNode as ITriggerNode).HookupTriggers();
                }
                else
                {
                    (BaseNode as ITriggerNode).RemoveTriggers();
                }
            }
        }

        public override void SetFieldValue(IAdvancedScriptInstance instance, INodeField containingField, ILaminarValue laminarValue, object value)
        {
            if (instance is null)
            {
                base.SetFieldValue(instance, containingField, laminarValue, value);
            }
            else
            {
                if (!_instanceManagers.TryGetValue(instance, out _))
                {
                    _instanceManagers.Add(instance, new InstanceManager(this, instance));
                }

                _instanceManagers[instance].SetValue(containingField, laminarValue, value);
            }
        }

        public override object GetFieldValue(IAdvancedScriptInstance instance, INodeField containingField, ILaminarValue laminarValue)
        {
            if (instance is not null && _instanceManagers.TryGetValue(instance, out InstanceManager manager))
            {
                return manager.GetValue(containingField, laminarValue);
            }

            return base.GetFieldValue(instance, containingField, laminarValue);
        }

        private void TriggerNode_Trigger(object sender, EventArgs e)
        {
             FlowOutContainer.OutputConnector?.Activate(null, Connection.PropagationDirection.Forwards);
        }

        private class InstanceManager : IDisposable
        {
            private readonly T _node;
            private readonly Dictionary<ILaminarValue, ILaminarValue> _instanceValues = new();
            private readonly INodeComponentList _nodeComponents;
            private readonly TriggerNode<T> _parentContainer;
            private readonly IAdvancedScriptInstance _instance;
            private bool _isLive;

            public InstanceManager(TriggerNode<T> parentContainer, IAdvancedScriptInstance instance)
            {
                _instance = instance;
                _parentContainer = parentContainer;
                _node = new();
                _nodeComponents = Constructor.NodeComponentList(_node.Fields);
                _nodeComponents.ParentNode = _parentContainer.BaseNode;
                (_node as ITriggerNode).Trigger += Trigger;
                _isLive = false;
                SetActive(_instance.IsActive.Value);
                _instance.IsActive.OnChange += SetActive;
                parentContainer.Update(instance);
            }

            private void SetActive(bool shouldBeActive)
            {
                if (shouldBeActive && !_isLive)
                {
                    (_node as ITriggerNode).HookupTriggers();
                    _isLive = true;
                }
                else if (!shouldBeActive && _isLive)
                {
                    (_node as ITriggerNode).RemoveTriggers();
                    _isLive = false;
                }
            }

            public void SetValue(INodeField containingField, ILaminarValue laminarValue, object value)
            {
                if (!_instanceValues.ContainsKey(laminarValue))
                {
                    RegisterValueInDictionary(containingField, laminarValue);
                }

                _instanceValues[laminarValue].Value = value;
                _parentContainer.Update(_instance);
            }

            public object GetValue(INodeField containingField, ILaminarValue laminarValue)
            {
                if (!_instanceValues.ContainsKey(laminarValue))
                {
                    RegisterValueInDictionary(containingField, laminarValue);
                }

                return _instanceValues[laminarValue].Value;
            }

            private void RegisterValueInDictionary(INodeField containingField, ILaminarValue toRegister)
            {
                INodeField myField = _nodeComponents[_parentContainer.FieldList.IndexOf(containingField)] as INodeField;
                foreach (var lamVal in containingField.AllValues)
                {
                    if (lamVal.Value == toRegister)
                    {
                        _instanceValues.Add(toRegister, myField.GetValue(lamVal.Key));
                        _instanceValues[toRegister].IsUserEditable.Value = false;
                    }
                }
            }

            private void Trigger(object sender, EventArgs e)
            {
                _parentContainer.FlowOutContainer?.OutputConnector.Activate(_instance, Connection.PropagationDirection.Forwards);
            }

            public void Dispose()
            {
                if (_instance.IsActive.Value)
                {
                    (_node as ITriggerNode).RemoveTriggers();
                }
            }
        }
    }
}
