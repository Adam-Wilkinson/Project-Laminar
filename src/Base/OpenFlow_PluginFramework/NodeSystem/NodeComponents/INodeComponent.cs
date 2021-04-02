using OpenFlow_PluginFramework.NodeSystem.Nodes;
using OpenFlow_PluginFramework.Primitives;
using System;
using System.Collections;
using System.ComponentModel;

namespace OpenFlow_PluginFramework.NodeSystem.NodeComponents
{
    public interface INodeComponent
    {
        bool IsVisible { get; set; }
        IOpacity Opacity { get; }
        INode ParentNode { get; set; }
        Action RemoveSelf { get; }
        IList VisualComponentList { get; }

        event PropertyChangedEventHandler PropertyChanged;
        event EventHandler<bool> VisibilityChanged;

        public void SetRemoveAction(Action<INodeComponent> removeAction);

        INodeComponent Clone();
    }
}