using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.Primitives;

namespace Laminar_Core.NodeSystem.NodeComponents.Visuals
{
    public class NodeLabel : VisualNodeComponent, INodeLabel
    {
        public NodeLabel(IObservableValue<string> name, IFlow flowInput, IFlow flowOutput, IOpacity opacity, IObservableValue<string> labelText) 
            : base(name, flowInput, flowOutput, opacity)
        {
            LabelText = labelText;
            LabelText.Value = Name.Value;
            Name.PropertyChanged += (o, e) =>
            {
                LabelText.Value = Name.Value;
            };
        }

        public override IVisualNodeComponent Clone()
        {
            INodeLabel output = Constructor.NodeLabel(Name.Value);
            CloneTo(output);
            return output;
        }

        public IObservableValue<string> LabelText { get; }
    }
}
