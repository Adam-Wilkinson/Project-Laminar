using OpenFlow_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core.Nodes.Connectors
{
    public interface IConnectionManager
    {
        IObservableValue<IConnector> InputConnector { get; }

        IObservableValue<IConnector> OutputConnector { get; }

        INodeBase ParentNode { get; set; }

        void UpdateInput();

        void UpdateOutput();

        void AddConnectionCheck<T>(Func<ConnectionType, T> connectionCheck) where T : IConnector;
    }
}
