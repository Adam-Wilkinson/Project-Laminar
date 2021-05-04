using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
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

        public void SetType(Type inputType)
        {
            _valueField.AddValue(INodeField.OutputKey, inputType, true);
        }
    }
}
