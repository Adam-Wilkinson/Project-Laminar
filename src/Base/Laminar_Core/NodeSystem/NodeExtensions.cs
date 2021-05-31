using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Scripting.Advanced.Editing;
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
        private static readonly Dictionary<Guid, IEditableNodeLabel> InputNameLabels = new();

        public static IEditableNodeLabel GetNameLabel(this INode node)
        {
            if (NameLabels.TryGetValue(node, out IEditableNodeLabel label))
            {
                return label;
            }

            if (node is InputNode inputNode)
            {
                if (InputNameLabels.TryGetValue(inputNode.InputID, out IEditableNodeLabel editableNodeLabel))
                {
                    return editableNodeLabel;
                }

                IEditableNodeLabel inputNodeLabel = Constructor.EditableNodeLabel(node.NodeName);
                inputNodeLabel.ParentNode = node;
                inputNodeLabel.IndexInParent = -1;
                InputNameLabels.Add(inputNode.InputID, inputNodeLabel);

                return inputNodeLabel;
            }

            IEditableNodeLabel output = Constructor.EditableNodeLabel(node.NodeName);
            output.ParentNode = node;
            output.IndexInParent = -1;
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

            if (nodeToClone is InputNode inputNodeToClone && newNode is InputNode newInputNode)
            {
                newInputNode.InputID = inputNodeToClone.InputID;
                newInputNode.LaminarValue = inputNodeToClone.LaminarValue;
                return newNode;
            }

            foreach ((INodeComponent CloneFrom, INodeComponent CloneTo) in nodeToClone.Fields.Zip(newNode.Fields))
            {
                CloneFrom.CloneTo(CloneTo);
                CloneTo.ParentNode = newNode;
            }

            return newNode;
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
