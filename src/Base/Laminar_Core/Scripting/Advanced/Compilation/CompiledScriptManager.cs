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

        public void SetScript(IAdvancedScript advancedScript)
        {
            _script = advancedScript;
            Refresh();
        }

        public void Refresh()
        {
            foreach (IAdvancedScriptInstance instance in _allInstances)
            {
                instance.CompiledScript = _compiler.Compile(_script);
            }
        }
    }
}
