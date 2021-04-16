using Laminar_Core.NodeSystem.Connection.ConnectorManagers;
using Laminar_PluginFramework.NodeSystem.NodeComponents.Visuals;
using Laminar_PluginFramework.Primitives;
using System.Collections.Generic;
using System.Linq;

namespace Laminar_Core.NodeSystem.Connection
{
    public class Connector : IConnector
    {
        private readonly List<INodeConnection> _connections = new();
        private readonly List<IConnectorManager> managers = IConnectorManager.AllImplementingTypes.Select(x => (IConnectorManager)Instance.Factory.CreateInstance(x)).ToList();
        private IConnectorManager _manager;

        public Connector(IObservableValue<string> hexColour, IObservableValue<bool> exists)
        {
            HexColour = hexColour;
            Exists = exists;
            Exists.Value = false;
        }

        public ConnectorType ConnectorType { get; set; }

        public bool IsExclusiveConnection => Manager.ConnectorExclusiveCheck();

        public INodeConnection ExclusiveConnection => IsExclusiveConnection && _connections.Count > 0 ? _connections[0] : null;

        public object Tag { get; set; }

        public IObservableValue<bool> Exists { get; }

        public IObservableValue<string> HexColour { get; }

        public IConnectorManager Manager
        {
            get => _manager;
            private set
            {
                if (Manager is not null)
                {
                    HexColour.RemoveDependency(Manager.HexColour);
                }

                _manager = value;

                if (Manager is not null)
                {
                    HexColour.AddDependency(Manager.HexColour);
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
            foreach (IConnectorManager manager in managers)
            {
                manager.Initialize(component, ConnectorType);
                manager.ExistsChanged += (o, e) => Manager = TryFindFormat();
            }

            Manager = TryFindFormat();
        }

        public void Activate() => Manager?.Activate();

        private IConnectorManager TryFindFormat()
        {
            foreach (IConnectorManager manager in managers)
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
