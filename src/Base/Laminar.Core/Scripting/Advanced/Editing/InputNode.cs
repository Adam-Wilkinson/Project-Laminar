using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;

namespace Laminar_Core.Scripting.Advanced.Editing
{
    public class InputNode : INode
    {
        private readonly INodeField _valueField = Constructor.NodeField("");

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return _valueField;
            }
        }

        public string NodeName { get; } = "Input";

        public Guid InputID { get; set; } = Guid.NewGuid();

        public object Value
        {
            get => LaminarValue.Value;
            set => LaminarValue.Value = value;
        }

        public ILaminarValue LaminarValue
        {
            get => _valueField.GetValue(INodeField.OutputKey);
            set => _valueField.AddValue(INodeField.OutputKey, value);
        }

        public void Evaluate()
        {
            throw new NotImplementedException();
        }

        public void SetType(Type inputType)
        {
            _valueField.AddValue(INodeField.OutputKey, inputType, true);
        }
    }
}
