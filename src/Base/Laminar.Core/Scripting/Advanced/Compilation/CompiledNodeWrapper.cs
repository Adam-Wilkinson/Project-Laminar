using Laminar_Core.NodeSystem;
using Laminar_Core.NodeSystem.NodeComponents.Visuals;
using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Scripting.Advanced.Editing;
using Laminar_Core.Scripting.Advanced.Editing.Connection;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laminar_Core.Scripting.Advanced.Compilation
{
    public class CompiledNodeWrapper : IDisposable
    {
        private readonly IAdvancedScriptCompiler _compilerInstance;
        private readonly List<CompiledNodeWrapper> _dependencies = new();
        public readonly List<CompiledNodeChain> flowOutChains = new();

        public static CompiledNodeWrapper Get(INodeContainer container, IAdvancedScriptCompiler compilerInstance)
        {
            if (compilerInstance.AllNodes.TryGetValue(container, out CompiledNodeWrapper wrapper))
            {
                return wrapper;
            }

            CompiledNodeWrapper newWrapper = new(container, compilerInstance);
            compilerInstance.AllNodes.Add(container, newWrapper);
            return newWrapper;
        }

        private CompiledNodeWrapper(INodeContainer container, IAdvancedScriptCompiler compilerInstance)
        {
            _compilerInstance = compilerInstance;
            if (container.CoreNode is InputNode inputNode && _compilerInstance.Inputs.TryGetValue(inputNode, out ILaminarValue value))
            {
                Outputs.Add(value);
            }
            else
            {
                CoreNode = container.GetCoreNodeInstance();
                SetupDependencies(container, CoreNode);
            }
        }

        public List<ILaminarValue> Outputs { get; } = new();

        public INode CoreNode { get; }

        public void Activate()
        {
            foreach (CompiledNodeWrapper dependentNode in _dependencies)
            {
                dependentNode.Activate();
            }

            if (CoreNode is IFunctionNode functionNode)
            {
                functionNode.Evaluate();
            }
        }

        public void Dispose()
        {
            foreach (CompiledNodeChain chain in flowOutChains)
            {
                chain.Dispose();
            }
        }

        private void SetupDependencies(INodeContainer rootNode, INode myNodeClone)
        {
            foreach ((var originalContainer, var clonedComponent) in ((IEnumerable<IVisualNodeComponentContainer>)rootNode.Fields).Zip(myNodeClone.GetVisualComponents()))
            {
                if (clonedComponent is INodeField clonedField)
                {
                    Outputs.Add(clonedField.GetValue(INodeField.OutputKey));
                    if (originalContainer.InputConnector.ExclusiveConnection is INodeConnection connection && !clonedField.FlowInput.Exists)
                    {
                        INodeContainer connectedNodeContainer = connection.OutputConnector.ConnectorNode;
                        int indexOfConnectedField = connection.OutputConnector.ParentComponentContainer.Child.IndexInParent;
                        CompiledNodeWrapper dependency = Get(connectedNodeContainer, _compilerInstance);
                        _dependencies.Add(dependency);

                        clonedField.GetValue(INodeField.InputKey).SetDependency(dependency.Outputs[indexOfConnectedField]);
                    }
                }

                if (originalContainer.Child.FlowOutput.Exists && originalContainer.OutputConnector.ExclusiveConnection is not null)
                {
                    flowOutChains.Add(new(originalContainer, clonedComponent.FlowOutput, _compilerInstance));
                }
            }
        }
    }
}
