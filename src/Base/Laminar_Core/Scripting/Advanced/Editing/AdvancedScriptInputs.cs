using Laminar_Core.NodeSystem;
using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.NodeSystem.Nodes.NodeTypes;
using Laminar_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Laminar_Core.Scripting.Advanced.Editing
{
    class AdvancedScriptInputs: IAdvancedScriptInputs
    {
        private readonly ObservableCollection<InputNode> _inputNodes = new();

        public AdvancedScriptInputs()
        {
            InputNodes = new(_inputNodes);
        }

        public bool Exists => InputNodes.Count > 0;

        public ReadOnlyObservableCollection<InputNode> InputNodes { get; }

        public void Add(Type valueType)
        {
            InputNode newNode = new();
            newNode.SetType(valueType);
            newNode.GetNameLabel().LabelText.Value = $"Input {_inputNodes.Count + 1}";
            _inputNodes.Add(newNode);
        }

        public void Add<T>()
        {
            Add(typeof(T));
        }
    }
}
