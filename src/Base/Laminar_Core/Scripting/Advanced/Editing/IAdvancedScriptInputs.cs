using Laminar_Core.NodeSystem.Nodes;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Laminar_Core.Scripting.Advanced.Editing
{
    public interface IAdvancedScriptInputs
    {
        event EventHandler<InputNode> NodeAdded;
        event EventHandler<InputNode> NodeRemoved;
        // ReadOnlyObservableCollection<InputNode> InputNodes { get; }

        int Count { get; }

        IEnumerable<InputNode> InputNodes { get; }

        bool Exists { get; }

        void Add(Type valueType);

        void Add(string name, object value);

        void Add<T>();
    }
}
