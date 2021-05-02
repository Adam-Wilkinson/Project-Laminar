using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Primitives.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsPluginBase.Window;

namespace WindowsPluginBase.Nodes
{
    public class GetWindowPos : IFunctionNode
    {
        private readonly INodeField _mainField = Constructor.NodeField("Window")
            .WithInput(Constructor.ManualTypeDefinitionManager().WithAcceptedDefinition(Constructor.TypeDefinition<WindowStub>(null)), false)
            .WithOutput(Constructor.ManualTypeDefinitionManager().WithAcceptedDefinition(Constructor.TypeDefinition<WindowLayout>(null)), false);

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return _mainField;
            }
        }

        public string NodeName { get; } = "Get Window Position";

        public void Evaluate()
        {
        }
    }
}
