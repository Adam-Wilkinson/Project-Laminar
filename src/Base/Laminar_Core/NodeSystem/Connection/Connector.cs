using Laminar_Core.NodeSystem.Connection.ConnectorManagers;
using Laminar_Core.NodeSystem.Nodes;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Laminar_Core.NodeSystem.Connection
{
    public class Connector : IConnector
    {
        private readonly List<INodeConnection> _connections = new();
        private readonly List<IConnectorManager> _managers;
        private IConnectorManager _manager;

        public Connector(IDependentValue<string> hexColour, IObservableValue<bool> exists, IObjectFactory factory)
        {
            HexColour = hexColour;
            Exists = exists;
            Exists.Value = false;
            _managers = IConnectorManager.AllImplementingTypes.Select(x => (IConnectorManager)factory.CreateInstance(x)).ToList();
        }

        public ConnectorType ConnectorType { get; set; }

        public bool IsExclusiveConnection => Manager.ConnectorExclusiveCheck();

        public INodeConnection ExclusiveConnection => IsExclusiveConnection && _connections.Count > 0 ? _connections[0] : null;

        public object Tag { get; set; }

        public IObservableValue<bool> Exists { get; }

        public IDependentValue<string> HexColour { get; }

        public IConnectorManager Manager
        {
            get => _manager;
            private set
            {
                if (Manager is not null)
                {
                    HexColour.RemoveDependency<string>();
                }

                _manager = value;

                if (Manager is not null)
                {
                    HexColour.SetDependency(Manager.HexColour);
                    Exists.Value = true;
                }
                else
                {
                    Exists.Value = false;
                }
            }
        }

        public bool CanConnectTo(IConnector toConnectTo)
        {
            return toConnectTo is Connector connector && Manager.CompatibilityCheck(connector.Manager);
        }

        public void AddConnection(INodeConnection connection)
        {
            if (ExclusiveConnection is not null)
            {
                ExclusiveConnection.Break();
            }

            Manager.ConnectionAddedAction((connection.Opposite(this) as Connector).Manager);

            _connections.Add(connection);
        }

        public void RemoveConnection(INodeConnection connection)
        {
            _connections.Remove(connection);
            Manager.ConnectionRemovedAction((connection.Opposite(this) as Connector).Manager);
        }

        public void Initialize(IVisualNodeComponent component)
        {
            foreach (IConnectorManager manager in _managers)
            {
                manager.Initialize(component, ConnectorType);
                manager.ExistsChanged += (o, e) => Manager = TryFindFormat();
            }

            Manager = TryFindFormat();
        }

        public void Activate() => Manager?.Activate();

        private IConnectorManager TryFindFormat()
        {
            foreach (IConnectorManager manager in _managers)
            {
                if (manager.ConnectorExists())
                {
                    return manager;
                }
            }

            return null;
        }
    }
}
