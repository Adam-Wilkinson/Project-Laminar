using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Scripting;
using Laminar_Core.Scripting.Advanced.Editing;
using Laminar_Core.Scripting.Advanced.Editing.Connection;
using Laminar_PluginFramework.Primitives;

namespace Laminar_Core.Scripting.Advanced
{
    public class AdvancedScript : IAdvancedScript
    {
        public AdvancedScript(IObservableValue<string> name, IAdvancedScriptEditor editor, IAdvancedScriptInputs inputs)
        {
            Editor = editor;
            Inputs = inputs;
            Name = name;
            Name.Value = "Advanced Script";
        }

        public IObservableValue<string> Name { get; }

        public IAdvancedScriptInputs Inputs { get; }

        public IAdvancedScriptEditor Editor { get; }
    }
}
