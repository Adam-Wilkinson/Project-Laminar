using System;

namespace OpenFlow_Core.NodeSystem.Connection
{
    public interface INodeConnection
    {
        event EventHandler OnBreak;

        IConnector InputConnector { get; }

        IConnector OutputConnector { get; }

        IConnector Opposite(IConnector connector);

        void Break();

        void Activate();
    }
}
