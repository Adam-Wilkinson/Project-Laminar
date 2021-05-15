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
        void Refresh(IAdvancedScript script);

        IAdvancedScriptInstance CreateInstance();

        void DisableAllScripts();

        void EnableAllScripts();
    }
}
