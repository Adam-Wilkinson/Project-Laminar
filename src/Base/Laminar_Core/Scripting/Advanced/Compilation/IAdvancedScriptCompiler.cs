using Laminar_Core.Scripting.Advanced.Editing;
using Laminar_Core.Scripting.Advanced.Instancing;

namespace Laminar_Core.Scripting.Advanced.Compilation
{
    public interface IAdvancedScriptCompiler
    {
        IAdvancedScriptInstance GetInstance();

        void SetScript(IAdvancedScript advancedScript);
    }
}
