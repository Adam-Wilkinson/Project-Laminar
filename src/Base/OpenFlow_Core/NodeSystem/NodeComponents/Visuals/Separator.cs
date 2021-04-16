using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.Primitives;

namespace Laminar_Core.NodeSystem.NodeComponents.Visuals
{
    public class Separator : VisualNodeComponent, ISeparator
    {
        public Separator(IObservableValue<string> name, IOpacity opacity)
            : base(name, opacity) { }

        public override INodeComponent Clone() => Instance.Factory.GetImplementation<ISeparator>();
    }
}
