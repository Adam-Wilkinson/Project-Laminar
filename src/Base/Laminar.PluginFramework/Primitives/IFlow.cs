using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using System;

namespace Laminar_PluginFramework.Primitives
{
    public interface IFlow
    {
        bool Exists { get; set; }

        IVisualNodeComponent ParentComponent { get; set; }

        event EventHandler<bool> ExistsChanged;

        event EventHandler Activated;

        void Activate();
    }
}
