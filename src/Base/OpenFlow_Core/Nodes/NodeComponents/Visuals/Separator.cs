using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.Primitives;

namespace OpenFlow_Core.Nodes.NodeComponents.Visuals
{
    public class Separator : VisualNodeComponent, ISeparator
    {
        public Separator(IObservableValue<string> name, IOpacity opacity)
            : base(name, opacity) { }

        public override INodeComponent Clone() => Instance.Factory.GetImplementation<ISeparator>();
    }
}
