using Laminar_Core.Primitives;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.NodeSystem.Nodes
{
    public class NodeDependencyAggregate
    {
        public NodeDependencyAggregate(IPoint location, IObservableValue<bool> errorState, IEditableNodeLabel name, IObjectFactory factory)
        {
            Location = location;
            ErrorState = errorState;
            Name = name;
            Factory = factory;
        }

        public IPoint Location { get; }

        public IObservableValue<bool> ErrorState { get; }

        public IEditableNodeLabel Name { get; }

        public IObjectFactory Factory { get; }

        public void Deconstruct(out IPoint Location, out IObservableValue<bool> ErrorState, out IEditableNodeLabel Name, out IObjectFactory Factory)
        {
            Location = this.Location;
            ErrorState = this.ErrorState;
            Name = this.Name;
            Factory = this.Factory;
        }
    }
}
