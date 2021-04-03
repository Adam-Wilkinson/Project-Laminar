namespace OpenFlow_Core.Nodes.Connectors
{
    using System.Collections.Generic;
    using OpenFlow_Core.Nodes;

    public abstract class Connector<T> : IConnector 
        where T : Connector<T>
    {
        protected Connector(INodeBase parent, ConnectionType connectionType)
        {
            ParentNode = parent;
            ConnectionType = connectionType;
        }

        public bool ConnectionDirty { get; private set; }

        public INodeBase ParentNode { get; set; }

        public object Tag { get; set; }

        public abstract bool IsExclusiveConnection { get; }

        public abstract string ColourHex { get; }

        public IConnector ExclusiveConnection
        {
            get => TypedExclusiveConnection;
        }

        public T TypedExclusiveConnection
        {
            get => IsExclusiveConnection && Connections.Count > 0 ? Connections[0] : null;
            set => Connections = new() { value };
        }

        public ConnectionType ConnectionType { get; }

        protected List<T> Connections { get; private set; } = new();

        public bool TryAddConnection(IConnector toAdd)
        {
            if (toAdd is T typedToAdd)
            {
                switch (GetConnectionStatus(typedToAdd))
                {
                    case ConnectionStatus.NeutralStatus:
                        if (CanAddConnection(typedToAdd))
                        {
                            goto case ConnectionStatus.MyUpdateTurn;
                        }

                        break;
                    case ConnectionStatus.MyUpdateTurn:
                        AddConnection(typedToAdd);
                        ConnectorAdded(typedToAdd);
                        ConnectionDirty = true;
                        typedToAdd.TryAddConnection(this);
                        return true;
                    case ConnectionStatus.UpdatesComplete:
                        ConnectionDirty = false;
                        typedToAdd.TryAddConnection(this);
                        break;
                    case ConnectionStatus.Cleanup:
                        ConnectionDirty = false;
                        break;
                }
            }

            return false;
        }

        public bool TryRemoveConnection(IConnector toRemove)
        {
            if (toRemove is T typedToRemove)
            {
                switch (GetConnectionStatus(typedToRemove))
                {
                    case ConnectionStatus.NeutralStatus:
                        if (Connections.Contains(typedToRemove))
                        {
                            goto case ConnectionStatus.MyUpdateTurn;
                        }

                        break;
                    case ConnectionStatus.MyUpdateTurn:
                        Connections.Remove(typedToRemove);
                        ConnectorRemoved(typedToRemove);
                        ConnectionDirty = true;
                        typedToRemove.TryRemoveConnection(this);
                        return true;
                    case ConnectionStatus.UpdatesComplete:
                        ConnectionDirty = false;
                        typedToRemove.TryRemoveConnection(this);
                        return true;
                    case ConnectionStatus.Cleanup:
                        ConnectionDirty = false;
                        return true;
                }
            }

            return false;
        }
        
        protected virtual bool CanAddConnection(T toConnect)
        {
            return (int)toConnect.ConnectionType + (int)ConnectionType == 3 && toConnect.GetType() == GetType();
        }

        protected virtual void ConnectorRemoved(T e)
        {
        }

        protected virtual void ConnectorAdded(T e)
        {
        }

        private ConnectionStatus GetConnectionStatus(T toConnect) => (ConnectionDirty, toConnect.ConnectionDirty) switch
        {
            (true, true) => ConnectionStatus.UpdatesComplete,
            (false, true) => ConnectionStatus.MyUpdateTurn,
            (true, false) => ConnectionStatus.Cleanup,
            (false, false) => ConnectionStatus.NeutralStatus,
        };

        private void AddConnection(T connector)
        {
            if (IsExclusiveConnection)
            {
                TypedExclusiveConnection = connector;
            }
            else
            {
                Connections.Add(connector);
            }
        }


        private enum ConnectionStatus
        {
            NeutralStatus,
            MyUpdateTurn,
            UpdatesComplete,
            Cleanup,
        }
    }
}
