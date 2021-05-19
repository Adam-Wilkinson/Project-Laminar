using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.NodeSystem;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Primitives;
using System.Collections.Generic;
using System.Linq;
using Laminar_Core.Scripting.Advanced.Editing;
using Laminar_Core.Primitives.LaminarValue;
using Laminar_Core.NodeSystem.NodeComponents.Visuals;
using Laminar_Core.Scripting.Advanced.Editing.Connection;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using System;

namespace Laminar_Core.Scripting.Advanced.Compilation
{
    public class AdvancedScriptCompiler : IAdvancedScriptCompiler
    {
        private readonly ILaminarValueFactory _valueFactory;
        private readonly IObjectFactory _factory;

        public AdvancedScriptCompiler(IObjectFactory factory, ILaminarValueFactory valueFactory)
        {
            _valueFactory = valueFactory;
            _factory = factory;
        }

        public Dictionary<InputNode, ILaminarValue> Inputs { get; private set; }

        public Dictionary<INodeContainer, CompiledNodeWrapper> AllNodes { get; } = new();

        public ICompiledScript Compile(IAdvancedScript script)
        {
            ICompiledScript compiledScript = _factory.CreateInstance<ICompiledScript>();
            compiledScript.OriginalScript = script;
            Inputs = new();

            foreach (InputNode scriptInput in script.Editor.Inputs.InputNodes)
            {
                ILaminarValue myInputValue = _valueFactory.Get(scriptInput.Value, true);
                myInputValue.Name = scriptInput.GetNameLabel().LabelText.Value;
                compiledScript.Inputs.Add(myInputValue);
                Inputs.Add(scriptInput, myInputValue);
            }

            foreach (INodeContainer triggerNode in script.Editor.TriggerNodes)
            {
                if (triggerNode.Name.OutputConnector.ExclusiveConnection is not null)
                {
                    CompiledNodeWrapper wrappedTrigger = CompiledNodeWrapper.Get(triggerNode, this);
                    compiledScript.AllTriggerNodes.Add(wrappedTrigger);
                    wrappedTrigger.flowOutChains.Add(new CompiledNodeChain(triggerNode.Name, wrappedTrigger.CoreNode.GetNameLabel().FlowOutput, this));
                }
            }

            return compiledScript;
        }
    }
}
