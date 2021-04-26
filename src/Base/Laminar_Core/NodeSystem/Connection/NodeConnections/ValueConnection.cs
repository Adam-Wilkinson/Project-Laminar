using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core.NodeSystem.Connection.NodeConnections
{
    public class ValueConnection : INodeConnection
    {
        public ValueConnection(IConnector outputConnector, IConnector inputConnector)
        {
            InputConnector = inputConnector;
            OutputConnector = outputConnector;
        }

        public IConnector InputConnector { get; }

        public IConnector OutputConnector { get; }

        public event EventHandler OnBreak;

        public void Activate()
        {
            throw new NotImplementedException();
        }

        public void Break()
        {
            throw new NotImplementedException();
        }

        public IConnector Opposite(IConnector connector)
        {
            throw new NotImplementedException();
        }
    }
}
