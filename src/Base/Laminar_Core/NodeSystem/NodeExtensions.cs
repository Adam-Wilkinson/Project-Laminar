using Laminar_Core.NodeSystem.Nodes;
using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.NodeSystem
{
    public static class NodeExtensions
    {
        private static readonly Dictionary<INode, IEditableNodeLabel> NameLabels = new();

        public static IEditableNodeLabel GetNameLabel(this INode node)
        {
            if (NameLabels.TryGetValue(node, out IEditableNodeLabel label))
            {
                return label;
            }

            IEditableNodeLabel output = Constructor.EditableNodeLabel(node.NodeName);
            output.ParentNode = node;
            NameLabels.Add(node, output);
            if (node is ITriggerNode triggerNode)
            {
                output.FlowOutput.Exists = true;
                triggerNode.Trigger += (o, e) => output.FlowOutput.Activate();
            }

            return output;
        }

        public static T Clone<T>(this T nodeToClone) where T : INode
        {
            T newNode = (T)Activator.CreateInstance(typeof(T));

            foreach ((INodeComponent CloneFrom, INodeComponent CloneTo) in nodeToClone.Fields.Zip(newNode.Fields))
            {
                CloneFrom.CloneTo(CloneTo);
                CloneTo.ParentNode = newNode;
            }

            return newNode;
        }

        public static void CopyComponents(this INode node, IEnumerable<INodeComponent> components)
        {
            foreach ((INodeComponent CloneFrom, INodeComponent CloneTo) in components.Zip(node.Fields))
            {
                CloneFrom.CloneTo(CloneTo);
                CloneTo.ParentNode = node;
            }
        }

        public static IEnumerable<IVisualNodeComponent> GetVisualComponents(this INode node)
        {
            foreach (INodeComponent component in node.Fields)
            {
                foreach (IVisualNodeComponent visualComponent in component.VisualComponentList)
                {
                    yield return visualComponent;
                }
            }
        }
    }
}
