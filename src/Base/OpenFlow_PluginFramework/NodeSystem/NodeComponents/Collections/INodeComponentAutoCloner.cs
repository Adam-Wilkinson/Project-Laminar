using Laminar_PluginFramework.NodeSystem.Nodes;
using System;

namespace Laminar_PluginFramework.NodeSystem.NodeComponents.Collections
{
    public interface INodeComponentAutoCloner : INodeComponentCollection
    {
        void ResetWith(INodeComponent originalClone, int minimumFieldCount, Func<int, string> nameRule);
    }
}