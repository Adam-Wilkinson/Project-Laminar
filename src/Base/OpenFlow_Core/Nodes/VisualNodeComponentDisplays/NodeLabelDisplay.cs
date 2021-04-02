using OpenFlow_Core.Nodes.NodeComponents.Visuals;
using OpenFlow_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core.Nodes.VisualNodeComponentDisplays
{
    public class NodeLabelDisplay : VisualNodeComponentDisplay<NodeLabel>
    {
        public NodeLabelDisplay(NodeBase parent, NodeLabel child) : base(parent, child)
        {
            LabelText = Instance.Factory.GetImplementation<IObservableValue<string>>();
            LabelText.Value = ChildComponent.Name;
            ChildComponent.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName is nameof(NodeLabel.Name))
                {
                    LabelText.Value = ChildComponent.Name;
                }
            };
        }

        public IObservableValue<string> LabelText { get; }
    }
}
