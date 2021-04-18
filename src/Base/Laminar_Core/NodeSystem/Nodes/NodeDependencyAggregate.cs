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
        public NodeDependencyAggregate(IPoint location, IObservableValue<bool> errorState, IObservableValue<string> name, IObjectFactory factory)
        {
            Location = location;
            ErrorState = errorState;
            Name = name;
            Factory = factory;
        }

        public IPoint Location { get; }

        public IObservableValue<bool> ErrorState { get; }

        public IObservableValue<string> Name { get; }

        public IObjectFactory Factory { get; }

        public void Deconstruct(out IPoint Location, out IObservableValue<bool> ErrorState, out IObservableValue<string> Name, out IObjectFactory Factory)
        {
            Location = this.Location;
            ErrorState = this.ErrorState;
            Name = this.Name;
            Factory = this.Factory;
        }
    }
}
