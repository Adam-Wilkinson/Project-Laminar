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
        private readonly ObservableCollection<INodeContainer> _inputNodes = new();
        private readonly INodeFactory _nodeFactory;

        public AdvancedScriptInputs(INodeFactory nodeFactory)
        {
            _nodeFactory = nodeFactory;
            InputNodes = new(_inputNodes);
        }

        public bool Exists => InputNodes.Count > 0;

        public ReadOnlyObservableCollection<INodeContainer> InputNodes { get; }

        public void Add(Type valueType)
        {
            InputNode newNode = new();
            newNode.SetType(valueType);
            INodeContainer newContainer = _nodeFactory.Get(newNode);
            newNode.GetNameLabel().LabelText.Value = $"Input {_inputNodes.Count + 1}";
            _inputNodes.Add(newContainer);
        }

        public void Add<T>()
        {
            Add(typeof(T));
        }
    }
}
