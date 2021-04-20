using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.NodeSystem.Nodes.NodeTypes;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Laminar_Core.NodeSystem.NodeTreeSystem
{
    class NodeTreeInputs : INodeTreeInputs
    {
        private readonly ObservableCollection<INodeBase> _inputNodes = new();
        private readonly INodeFactory _nodeFactory;

        public NodeTreeInputs(INodeFactory nodeFactory)
        {
            _nodeFactory = nodeFactory;
            InputNodes = new(_inputNodes);
        }

        public ReadOnlyObservableCollection<INodeBase> InputNodes { get; }

        public void Add(Type valueType)
        {
            InputNodeBase newNode = _nodeFactory.Get<InputNode>() as InputNodeBase;
            newNode.NameLabel.LabelText.Value = $"Input {_inputNodes.Count + 1}";
            newNode.SetType(valueType);
            _inputNodes.Add(newNode);
        }

        public void Add<T>()
        {
            Add(typeof(T));
        }
    }
}
