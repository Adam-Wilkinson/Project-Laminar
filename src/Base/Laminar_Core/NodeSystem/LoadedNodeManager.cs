﻿using System.Collections.Generic;
using System.Linq;
using Laminar_Core.NodeSystem.Nodes;
using Laminar_PluginFramework.NodeSystem.Nodes;

namespace Laminar_Core.NodeSystem
{
    public class LoadedNodeManager
    {
        private readonly INodeFactory _nodeFactory;

        public LoadedNodeManager(INodeFactory nodeFactory)
        {
            _nodeFactory = nodeFactory;
        }
        public NodeCatagories LoadedNodes { get; } = new(0);

        public void AddNodeToCatagory<TNode>(TNode newNode, string catagoryName, string subCatagoryName = null)
            where TNode : INode, new()
        {
            INodeContainer container = _nodeFactory.Get(newNode);
            LoadedNodes.PlaceNode(new string[] { catagoryName, subCatagoryName }, container);
        }

        public class NodeCatagories
        {
            public NodeCatagories(int index)
            {
                Index = index;
            }

            public Dictionary<string, NodeCatagories> SubCatagories { get; private set; } = new Dictionary<string, NodeCatagories>();

            public int Index { get; }

            public List<INodeContainer> Nodes { get; } = new();

            public List<INodeContainer> FirstGroup()
            {
                if (Nodes.Count > 0)
                {
                    return Nodes;
                }
                else
                {
                    return SubCatagories.First().Value.FirstGroup();
                }
            }

            public void Sort()
            {
                SubCatagories = new(SubCatagories.OrderBy(x => x.Key));
                foreach (var kvp in SubCatagories)
                {
                    kvp.Value.Sort();
                }
            }

            public void PlaceNode(IEnumerable<string> catagoryPath, INodeContainer node)
            {
                if (!catagoryPath.Any() || catagoryPath.First() == null || catagoryPath.First() == "")
                {
                    Nodes.Add(node);
                }
                else
                {
                    if (!SubCatagories.ContainsKey(catagoryPath.First()))
                    {
                        SubCatagories.Add(catagoryPath.First(), new NodeCatagories(SubCatagories.Count));
                    }

                    SubCatagories[catagoryPath.First()].PlaceNode(catagoryPath.Skip(1), node);
                }
            }
        }
    }
}
