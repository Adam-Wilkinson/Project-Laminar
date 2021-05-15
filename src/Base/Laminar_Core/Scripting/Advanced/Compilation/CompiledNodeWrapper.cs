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

        public CompiledNodeWrapper(INodeContainer container, IAdvancedScriptCompiler compilerInstance, int indexOfRequestedOutput = -1)
        {
            _compilerInstance = compilerInstance;
            CoreNode = container.GetCoreNodeInstance();
            DependencyValue = SetupDependencies(container, CoreNode, indexOfRequestedOutput);
        }

        public INode CoreNode { get; }

        public ILaminarValue DependencyValue { get; }

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

        private ILaminarValue SetupDependencies(INodeContainer rootNode, INode myNodeClone, int indexOfRequestedOutput = -1)
        {
            if (rootNode.CoreNode is InputNode inputNode && _compilerInstance.Inputs.TryGetValue(inputNode, out ILaminarValue value))
            {
                return value;
            }

            ILaminarValue output = null;
            int index = 0;
            foreach ((var originalContainer, var clonedComponent) in ((IEnumerable<IVisualNodeComponentContainer>)rootNode.Fields).Zip(myNodeClone.GetVisualComponents()))
            {
                if (clonedComponent is INodeField clonedField && originalContainer.InputConnector.ExclusiveConnection is INodeConnection connection && !clonedField.FlowInput.Exists)
                {
                    INodeContainer connectedNodeContainer = connection.OutputConnector.ConnectorNode;
                    int indexOfConnectedField = connection.OutputConnector.ParentComponentContainer.Child.IndexInParent;
                    CompiledNodeWrapper dependency = new(connectedNodeContainer, _compilerInstance, indexOfConnectedField);
                    _dependencies.Add(dependency);

                    clonedField.GetValue(INodeField.InputKey).SetDependency(dependency.DependencyValue);

                    if (index == indexOfRequestedOutput)
                    {
                        output = clonedField.GetValue(INodeField.OutputKey);
                    }
                }

                if (originalContainer.Child.FlowOutput.Exists && originalContainer.OutputConnector.ExclusiveConnection is not null)
                {
                    flowOutChains.Add(new(originalContainer, clonedComponent.FlowOutput, _compilerInstance));
                }
                index++;
            }

            return output;
        }
    }
}
