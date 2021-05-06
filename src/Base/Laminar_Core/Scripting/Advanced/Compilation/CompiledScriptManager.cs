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
        private readonly List<AdvancedScriptInstance> _allInstances = new();
        private readonly IObjectFactory _factory;
        private IAdvancedScript _script;
        private ICompiledScript _compiledScript;

        public CompiledScriptManager(IObjectFactory factory, IAdvancedScriptCompiler compiler)
        {
            _factory = factory;
            _compiler = compiler;
        }

        public IAdvancedScriptInstance CreateInstance()
        {
            AdvancedScriptInstance newInstance = _factory.CreateInstance<AdvancedScriptInstance>();
            _allInstances.Add(newInstance);
            return newInstance;
        }

        public void DisableAllScripts()
        {
            foreach (AdvancedScriptInstance instance in _allInstances)
            {
                instance.IsActive.Value = false;
            }
        }

        public void EnableAllScripts()
        {
            foreach (AdvancedScriptInstance instance in _allInstances)
            {
                instance.IsActive.Value = true;
            }
        }

        public void SetScript(IAdvancedScript advancedScript)
        {
            _script = advancedScript;
            Refresh();
        }

        public void Refresh()
        {
            _compiledScript = _compiler.Compile(_script);
            foreach (AdvancedScriptInstance instance in _allInstances)
            {
                instance.UpdateScript(_compiledScript);
            }
        }
    }
}
