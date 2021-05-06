using Laminar_Core.Scripting.Advanced.Instancing;
using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Scripting.Advanced.Compilation
{
    class AdvancedScriptCompiler : IAdvancedScriptCompiler
    {
        private readonly List<IAdvancedScriptInstance> _allInstances = new();
        private readonly IObjectFactory _factory;
        private IAdvancedScript _script;

        public AdvancedScriptCompiler(IObjectFactory factory)
        {
            _factory = factory;
        }

        public IAdvancedScriptInstance CreateInstance()
        {
            Debug.WriteLine("Creating instance of my script");
            IAdvancedScriptInstance newInstance = _factory.CreateInstance<IAdvancedScriptInstance>();
            return newInstance;
        }

        public void SetScript(IAdvancedScript advancedScript)
        {
            _script = advancedScript;
        }
    }
}
