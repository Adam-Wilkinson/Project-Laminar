using Laminar_Core.Scripting.Advanced.Instancing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.Scripting.Advanced.Compilation
{
    public interface ICompiledScriptManager
    {
        IAdvancedScriptInstance CreateInstance();
        void SetScript(IAdvancedScript advancedScript);
        void DisableAllScripts();
        void Refresh();
        void EnableAllScripts();
    }
}
