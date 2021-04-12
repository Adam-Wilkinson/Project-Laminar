﻿using OpenFlow_PluginFramework;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.Primitives;

namespace OpenFlow_Core.NodeSystem.NodeComponents.Visuals
{
    public class NodeLabel : VisualNodeComponent, INodeLabel
    {
        public NodeLabel(IObservableValue<string> name, IOpacity opacity, IObservableValue<string> labelText) 
            : base(name, opacity)
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