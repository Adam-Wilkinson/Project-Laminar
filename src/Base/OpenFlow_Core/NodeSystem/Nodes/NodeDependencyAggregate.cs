using Laminar_Core.Primitives;
using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.NodeSystem.Nodes
{
    public class NodeDependencyAggregate : ObjectFactory.IDependencyAggregate
    {
        public NodeDependencyAggregate(IPoint location, IObservableValue<bool> errorState, IObservableValue<string> name)
        {
            Location = location;
            ErrorState = errorState;
            Name = name;
        }

        public IPoint Location { get; }

        public IObservableValue<bool> ErrorState { get; }

        public IObservableValue<string> Name { get; }

        public void Deconstruct(out IPoint Location, out IObservableValue<bool> ErrorState, out IObservableValue<string> Name)
        {
            Location = this.Location;
            ErrorState = this.ErrorState;
            Name = this.Name;
        }
    }
}
