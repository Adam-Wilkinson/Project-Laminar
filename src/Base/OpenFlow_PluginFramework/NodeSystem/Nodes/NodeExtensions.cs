namespace OpenFlow_PluginFramework.NodeSystem.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;

    public static class NodeExtensions
    {
        private static readonly Dictionary<INode, Action> TriggerNodeEvaluate = new();

        public static void SubscribeToEvaluate(this INode node, Action onEvaluate)
        {
            if (!TriggerNodeEvaluate.ContainsKey(node))
            {
                TriggerNodeEvaluate.Add(node, new Action(() => { }));
            }

            TriggerNodeEvaluate[node] += onEvaluate;
        }

        public static void TriggerEvaluate(this INode node)
        {
            if (node != null && TriggerNodeEvaluate.TryGetValue(node, out Action trigger))
            {
                trigger?.Invoke();
            }
        }
    }
}
