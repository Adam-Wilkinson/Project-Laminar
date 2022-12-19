using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.Primitives;

namespace Laminar_Core.NodeSystem.NodeComponents.Visuals
{
    public class Separator : VisualNodeComponent, ISeparator
    {
        private readonly IObjectFactory _factory;

        public Separator(IObservableValue<string> name, IFlow flowInput, IFlow flowOutput, IOpacity opacity, IObjectFactory factory)
            : base(name, flowInput, flowOutput, opacity) 
        {
            _factory = factory;
        }

        public override INodeComponent Clone() => _factory.CreateInstance<Separator>();
    }
}