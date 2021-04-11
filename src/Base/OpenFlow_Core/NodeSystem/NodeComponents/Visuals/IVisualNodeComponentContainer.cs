using OpenFlow_Core.NodeSystem.Connection;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.Primitives;
using System;

namespace OpenFlow_Core.NodeSystem.NodeComponents.Visuals
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
