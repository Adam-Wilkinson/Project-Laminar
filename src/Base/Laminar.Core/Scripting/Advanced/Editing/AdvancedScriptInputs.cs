using Laminar_Core.NodeSystem;
using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.NodeSystem.Nodes.NodeTypes;
using Laminar_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Laminar_Core.Scripting.Advanced.Editing
{
    class AdvancedScriptInputs: IAdvancedScriptInputs
    {
        private readonly List<InputNode> _inputNodes = new();

        public bool Exists => Count > 0;

        public int Count => _inputNodes.Count;

        public IEnumerable<InputNode> InputNodes => _inputNodes;

        public event EventHandler<InputNode> NodeAdded;
        public event EventHandler<InputNode> NodeRemoved;

        public void Add(Type valueType)
        {
            InputNode newNode = new();
            newNode.SetType(valueType);
            newNode.GetNameLabel().LabelText.Value = $"Input {_inputNodes.Count + 1}";
            _inputNodes.Add(newNode);
            NodeAdded?.Invoke(this, newNode);
        }

        public void Add(string name, object value)
        {
            InputNode newNode = new();
            newNode.SetType(value.GetType());
            newNode.Value = value;
            newNode.GetNameLabel().LabelText.Value = name;
            _inputNodes.Add(newNode);
            NodeAdded?.Invoke(this, newNode);
        }

        public void Add<T>()
        {
            Add(typeof(T));
        }
    }
}
