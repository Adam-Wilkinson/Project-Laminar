using Laminar_Core.NodeSystem;
using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Scripting.Advanced.Compilation;
using Laminar_Core.Scripting.Advanced.Editing;
using Laminar_PluginFramework;
using Laminar_PluginFramework.NodeSystem.NodeComponents;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Collections;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Primitives;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Laminar_Core.Scripting.Advanced.Instancing
{
    public class AdvancedScriptInputsNode : INode
    {
        private readonly INodeComponentList AllInputs = Constructor.NodeComponentList();

        public void SetInstance(ICompiledScript script)
        {
            NodeName = script.OriginalScript.Name.Value;
            foreach (ILaminarValue input in script.Inputs)
            {
                AllInputs.Add(Constructor.NodeField(input.Name).WithValue("Display", input));
            }
        }

        public void Evaluate()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<INodeComponent> Fields
        {
            get
            {
                yield return AllInputs;
            }
        }

        public string NodeName { get; set; }
    }
}
