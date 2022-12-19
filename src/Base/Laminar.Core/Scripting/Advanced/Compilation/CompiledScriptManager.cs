using Laminar_Core.Scripting.Advanced.Editing;
using Laminar_Core.Scripting.Advanced.Instancing;
using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Scripting.Advanced.Compilation
{
    public class CompiledScriptManager : ICompiledScriptManager
    {
        private readonly IAdvancedScriptCompiler _compiler;
        private readonly List<IAdvancedScriptInstance> _allInstances = new();
        private readonly IObjectFactory _factory;
        private Guid[] _inputsOrder;
        private IAdvancedScript _script;

        public CompiledScriptManager(IObjectFactory factory, IAdvancedScriptCompiler compiler)
        {
            _factory = factory;
            _compiler = compiler;
        }

        public IAdvancedScriptInstance CreateInstance()
        {
            IAdvancedScriptInstance newInstance = _factory.CreateInstance<IAdvancedScriptInstance>();
            _allInstances.Add(newInstance);
            newInstance.Name.Value = _script.Name.Value;
            newInstance.CompiledScript = _compiler.Compile(_script);
            return newInstance;
        }

        public void Refresh(IAdvancedScript script)
        {
            var inputLocationChanges = new List<(int indexInNew, int indexInOld)>();
            Guid[] newInputsOrder = new Guid[script.Editor.Inputs.Count];
            _script = script;

            int indexInNew = 0;
            int indexInOld = 0;
            foreach (InputNode inputNode in script.Editor.Inputs.InputNodes)
            {
                if (_inputsOrder is not null)
                {
                    foreach (Guid guid in _inputsOrder)
                    {
                        if (inputNode.InputID == guid)
                        {
                            inputLocationChanges.Add((indexInNew, indexInOld));
                        }
                        indexInOld++;
                    }
                }
                newInputsOrder[indexInNew] = inputNode.InputID;
                indexInNew++;
                indexInOld = 0;
            }
            _inputsOrder = newInputsOrder;

            foreach (IAdvancedScriptInstance instance in _allInstances)
            {
                List<ILaminarValue> oldInputs = instance.CompiledScript.Inputs;
                instance.CompiledScript = _compiler.Compile(script);
                foreach (var locationChange in inputLocationChanges)
                {
                    instance.CompiledScript.Inputs[locationChange.indexInNew].Value = oldInputs[locationChange.indexInOld].Value;
                }
            }
        }

        public void DisableAllScripts()
        {
            foreach (IAdvancedScriptInstance instance in _allInstances)
            {
                instance.IsActive.Value = false;
            }
        }

        public void EnableAllScripts()
        {
            foreach (IAdvancedScriptInstance instance in _allInstances)
            {
                instance.IsActive.Value = true;
            }
        }
    }
}
