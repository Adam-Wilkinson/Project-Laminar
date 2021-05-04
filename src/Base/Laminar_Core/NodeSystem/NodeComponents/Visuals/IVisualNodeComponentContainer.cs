using Laminar_Core.Scripting.Advanced.Editing.Connection;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.Primitives;
using System;

namespace Laminar_Core.NodeSystem.NodeComponents.Visuals
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
