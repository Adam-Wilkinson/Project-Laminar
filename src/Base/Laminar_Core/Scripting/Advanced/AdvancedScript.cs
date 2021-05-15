using Laminar_Core.Scripting.Advanced.Compilation;
using Laminar_Core.Scripting.Advanced.Editing;
using Laminar_Core.Scripting.Advanced.Instancing;
using Laminar_PluginFramework.Primitives;

namespace Laminar_Core.Scripting.Advanced
{
    public class AdvancedScript : IAdvancedScript
    {
        private readonly ICompiledScriptManager _compilerManager;
        private bool _isBeingEdited;

        public AdvancedScript(IObservableValue<string> name, ICompiledScriptManager compilationManager, IAdvancedScriptEditor editor, IAdvancedScriptInputs inputs)
        {
            _compilerManager = compilationManager;
            _compilerManager.SetScript(this);

            Editor = editor;
            Inputs = inputs;
            Name = name;
            Name.Value = "Advanced Script";
        }

        public IObservableValue<string> Name { get; }

        public IAdvancedScriptInputs Inputs { get; }

        public IAdvancedScriptEditor Editor { get; }

        public bool IsBeingEdited
        {
            get => _isBeingEdited;
            set
            {
                if (_isBeingEdited == value)
                {
                    return;
                }

                _isBeingEdited = value;
                if (_isBeingEdited)
                {
                    _compilerManager.DisableAllScripts();
                } 
                else  
                {
                    _compilerManager.Refresh();
                    _compilerManager.EnableAllScripts();
                }

                Editor.IsLive = _isBeingEdited;
            }
        }

        public IAdvancedScriptInstance CreateInstance()
        {
            IAdvancedScriptInstance newInstance = _compilerManager.CreateInstance();
            return newInstance;
        }
    }
}
