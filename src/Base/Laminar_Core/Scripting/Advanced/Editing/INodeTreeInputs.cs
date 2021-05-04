using Laminar_Core.NodeSystem.Nodes;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using System;
using System.Collections.ObjectModel;

namespace Laminar_Core.Scripting.Advanced.Editing
{
    public interface IAdvancedScriptInputs

    {
        ReadOnlyObservableCollection<INodeContainer> InputNodes { get; }

        bool Exists { get; }

        void Add(Type valueType);

        void Add<T>();
    }
}
