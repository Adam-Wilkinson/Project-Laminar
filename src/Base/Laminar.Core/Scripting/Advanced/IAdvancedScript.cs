using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.Scripting;
using Laminar_Core.Scripting.Advanced.Editing;
using Laminar_Core.Scripting.Advanced.Editing.Connection;
using Laminar_Core.Scripting.Advanced.Instancing;
using Laminar_PluginFramework.Primitives;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Laminar_Core.Scripting.Advanced
{
    public interface IAdvancedScript
    {
        IAdvancedScriptEditor Editor { get; }

        bool IsBeingEdited { get; set; }

        IObservableValue<string> Name { get; }

        ReadOnlyDictionary<string, InputNode> Inputs { get; }

        void UpdateInputs();

        IAdvancedScriptInstance CreateInstance();
    }
}