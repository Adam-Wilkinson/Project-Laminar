using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Scripting.Advanced.Compilation;
using Laminar_Core.Scripting.Advanced.Editing;
using System;

namespace Laminar_Core.Scripting.Advanced.Instancing
{
    public class AdvancedScriptInstance : ScriptInstance, IAdvancedScriptInstance
    {
        private readonly INodeFactory _nodeFactory;
        private ICompiledScript _compiledScript;

        public AdvancedScriptInstance(ScriptDependencyAggregate deps, INodeFactory nodeFactory) : base(deps)
        {
            IsActive.Value = false;
            _nodeFactory = nodeFactory;
            ActiveChanged(IsActive.Value);
            IsActive.OnChange += ActiveChanged;
        }

        public INodeContainer Inputs { get; private set; }

        public ICompiledScript CompiledScript 
        { 
            get => _compiledScript; 
            set
            {
                _compiledScript = value;
                AdvancedScriptInputsNode inputsCoreNode = new();
                inputsCoreNode.SetInstance(_compiledScript);
                Inputs = _nodeFactory.Get(inputsCoreNode);
            }
        }

        public void UpdateScript(ICompiledScript newScript)
        {
            AdvancedScriptInputsNode inputsCoreNode = new();
            inputsCoreNode.SetInstance(newScript);
            Inputs = _nodeFactory.Get(inputsCoreNode);
        }


        private void ActiveChanged(bool isActive)
        {
            if (_compiledScript is not null)
            {
                _compiledScript.IsLive = isActive;
            }
        }
    }
}
