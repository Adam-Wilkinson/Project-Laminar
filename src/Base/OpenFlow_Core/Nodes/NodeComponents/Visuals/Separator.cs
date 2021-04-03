using OpenFlow_Core.Nodes.Connectors;
using OpenFlow_Core.Primitives;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core.Nodes.NodeComponents.Visuals
{
    public class Separator : DisplayableNodeComponent, ISeparator
    {
        public Separator(IObservableValue<string> name, IOpacity opacity, IConnectionManager connectionManager)
            : base(name, opacity, connectionManager) { }

        public override INodeComponent Clone() => Instance.Factory.GetImplementation<ISeparator>();
    }
}
