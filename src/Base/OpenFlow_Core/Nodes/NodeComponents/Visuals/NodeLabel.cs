using OpenFlow_Core.Nodes.Connectors;
using OpenFlow_Core.Primitives;
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
    public class NodeLabel : DisplayableNodeComponent, INodeLabel
    {
        public NodeLabel(IObservableValue<string> name, IOpacity opacity, IConnectionManager connectionManager, IObservableValue<string> labelText) 
            : base(name, opacity, connectionManager) 
        {
            LabelText = labelText;
            LabelText.Value = Name.Value;
            Name.PropertyChanged += (o, e) =>
            {
                LabelText.Value = Name.Value;
            };
        }

        public override IVisualNodeComponent Clone() => CloneTo(Constructor.NodeLabel(Name.Value));

        public IObservableValue<string> LabelText { get; }
    }
}
