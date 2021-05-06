using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Scripting;
using Laminar_Core.Scripting.Advanced.Compilation;
using Laminar_Core.Scripting.Advanced.Editing;
using Laminar_Core.Scripting.Advanced.Editing.Connection;
using Laminar_Core.Scripting.Advanced.Instancing;
using Laminar_PluginFramework.Primitives;

namespace Laminar_Core.Scripting.Advanced
{
    public class AdvancedScript : IAdvancedScript
    {
        private readonly IAdvancedScriptCompiler _compiler;

        public AdvancedScript(IObservableValue<string> name, IAdvancedScriptCompiler compiler, IAdvancedScriptEditor editor, IAdvancedScriptInputs inputs)
        {
            _compiler = compiler;
            _compiler.SetScript(this);

            Editor = editor;
            Inputs = inputs;
            Name = name;
            Name.Value = "Advanced Script";
        }

        public IObservableValue<string> Name { get; }

        public IAdvancedScriptInputs Inputs { get; }

        public IAdvancedScriptEditor Editor { get; }

        public IAdvancedScriptInstance CreateInstance()
        {
            IAdvancedScriptInstance newInstance = _compiler.CreateInstance();
            newInstance.Name.Value = Name.Value;
            return newInstance;
        }
    }
}
