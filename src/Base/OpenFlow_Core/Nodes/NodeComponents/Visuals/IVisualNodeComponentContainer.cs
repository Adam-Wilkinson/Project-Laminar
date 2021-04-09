using OpenFlow_Core.Nodes.Connection;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.Primitives;
using System;

namespace OpenFlow_Core.Nodes.NodeComponents.Visuals
{
    public interface IVisualNodeComponentContainer
    {
        IOpacity Opacity { get; }

        IVisualNodeComponent Child { get; set; }

        IConnector InputConnector { get; }

        IConnector OutputConnector { get; }

        bool HasRemoveFunction { get; }

        Action RemoveAction { get; }
    }
}
