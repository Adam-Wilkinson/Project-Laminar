﻿using OpenFlow_Core.Nodes.Connection.ConnectorManagers;
using OpenFlow_PluginFramework.NodeSystem.NodeComponents.Visuals;
using OpenFlow_PluginFramework.Primitives;
using System.Collections.Generic;
using System.Linq;

namespace OpenFlow_Core.Nodes.Connection
{
    public class Connector : IConnector
    {
        private readonly List<INodeConnection> _connections = new();
        private readonly IEnumerable<IConnectorManager> managers = IConnectorManager.AllImplementingTypes.Select(x => (IConnectorManager)Instance.Factory.CreateInstance(x));

        public Connector(IObservableValue<string> hexColour, IObservableValue<bool> exists)
        {
            HexColour = hexColour;
            Exists = exists;
            Exists.Value = false;
        }

        public IConnectorManager ConnectionFormat { get; private set; }

        public ConnectorType ConnectorType { get; set; }

        public bool IsExclusiveConnection => ConnectionFormat.ConnectorExclusiveCheck(ConnectorType);

        public INodeConnection ExclusiveConnection => IsExclusiveConnection && _connections.Count > 0 ? _connections[0] : null;

        public object Tag { get; set; }

        public IObservableValue<bool> Exists { get; }

        public IObservableValue<string> HexColour { get; }

        public bool CanConnectTo(IConnector toConnectTo)
        {
            return toConnectTo is Connector connector && ConnectionFormat.CompatibilityCheck(connector.ConnectionFormat);
        }

        public void AddConnection(INodeConnection connection)
        {
            if (ExclusiveConnection is not null)
            {
                ExclusiveConnection.Break();
            }

            ConnectionFormat.ConnectionAddedAction((connection.Opposite(this) as Connector).ConnectionFormat, ConnectorType);

            _connections.Add(connection);
        }

        public void RemoveConnection(INodeConnection connection)
        {
            _connections.Remove(connection);
            ConnectionFormat.ConnectionRemovedAction((connection.Opposite(this) as Connector).ConnectionFormat, ConnectorType);
        }

        public void HookupRefreshTriggers(IVisualNodeComponent component)
        {
            foreach (IConnectorManager manager in managers)
            {
                manager.HookupExistsCheck(component, ConnectorType);
                manager.ExistsChanged += (o, e) => SetConnectionFormat(TryFindFormat(component));
            }

            SetConnectionFormat(TryFindFormat(component));
        }

        private void SetConnectionFormat(IConnectorManager connectionFormat)
        {
            if (ConnectionFormat is not null)
            {
                HexColour.RemoveDependency(ConnectionFormat.HexColour);
            }

            ConnectionFormat = connectionFormat;

            if (ConnectionFormat is not null)
            {
                HexColour.AddDependency(ConnectionFormat.HexColour);
                Exists.Value = true;
            }
            else
            {
                Exists.Value = false;
            }
        }

        private IConnectorManager TryFindFormat(IVisualNodeComponent component)
        {
            foreach (IConnectorManager manager in managers)
            {
                if (manager.CheckIfConnectorExists(component, ConnectorType))
                {
                    return manager;
                }
            }

            return null;
        }
    }
}