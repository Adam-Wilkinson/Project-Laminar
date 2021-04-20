using Laminar_Core.NodeSystem.Nodes;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using System;
using System.Collections.ObjectModel;

namespace Laminar_Core.NodeSystem.NodeTreeSystem
{
    public interface INodeTreeInputs
    {
        ReadOnlyObservableCollection<INodeBase> InputNodes { get; }

        void Add(Type valueType);

        void Add<T>();
    }
}
