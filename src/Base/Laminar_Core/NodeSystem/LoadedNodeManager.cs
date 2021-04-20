using System.Collections.Generic;
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

        public void AddNodeToCatagory<TNode>(string catagoryName, string subCatagoryName = null)
            where TNode : INode, new()
        {
            LoadedNodes.PlaceNode(new string[] { catagoryName, subCatagoryName }, _nodeFactory.Get<TNode>());
        }

        public class NodeCatagories
        {
            public NodeCatagories(int index)
            {
                Index = index;
            }

            public Dictionary<string, NodeCatagories> SubCatagories { get; } = new Dictionary<string, NodeCatagories>();

            public int Index { get; }

            public List<INodeBase> Nodes { get; } = new();

            public List<INodeBase> FirstGroup()
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

            public void PlaceNode(IEnumerable<string> catagoryPath, INodeBase node)
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
