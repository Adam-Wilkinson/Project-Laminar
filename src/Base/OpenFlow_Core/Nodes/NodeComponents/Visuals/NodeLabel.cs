using OpenFlow_PluginFramework;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core.Nodes.NodeComponents.Visuals
{
    public class NodeLabel : VisualNodeComponent, INodeLabel
    {
        public NodeLabel(IOpacity opacity, IObservableValue<string> labelText) : base(opacity) 
        {
            LabelText = labelText;
            LabelText.Value = Name;
            PropertyChanged += (o, e) =>
            {
                if (e.PropertyName is nameof(INodeLabel.Name))
                {
                    LabelText.Value = Name;
                }
            };
        }

        public override IVisualNodeComponent Clone() => CloneTo(Constructor.NodeLabel(Name));

        public IObservableValue<string> LabelText { get; }
    }
}
