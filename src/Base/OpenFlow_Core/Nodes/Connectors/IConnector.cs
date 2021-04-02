using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFlow_Core.Nodes.Connectors
{
    public interface IConnector
    {
        public string ColourHex { get; }

        public NodeBase ParentNode { get; }

        public object Tag { get; set; }

        public ConnectionType ConnectionType { get; }

        public IConnector ExclusiveConnection { get; }

        public bool IsExclusiveConnection { get; }

        public bool TryAddConnection(IConnector connector);

        public bool TryRemoveConnection(IConnector connector);
    }
}
