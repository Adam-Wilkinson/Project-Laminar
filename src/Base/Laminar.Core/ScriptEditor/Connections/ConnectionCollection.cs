using System;
using System.Collections.Generic;
using Laminar.Contracts.ActionSystem;
using Laminar.Contracts.NodeSystem.Connection;
using Laminar.Core.ScriptEditor.Actions;
using Laminar.Domain;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

namespace Laminar.Core.ScriptEditor.Connections;

internal class ConnectionCollection : ObservableCollection<IConnection>, IConnectionCollection
{
    private readonly Dictionary<IIOConnector, List<IConnection>> _connections = new();
    private readonly IUserActionManager _actionManager;

    public ConnectionCollection(IUserActionManager userActionManager)
    {
        _actionManager = userActionManager;
    }

    public IReadOnlyList<IConnection> GetConnectionsTo(IIOConnector ioConnector)
    {
        if (!_connections.ContainsKey(ioConnector))
        {
            return Array.Empty<IConnection>();
        }

        return _connections[ioConnector];
    }

    public void RemoveConnectionsTo(IIOConnector connector)
    {
        if (connector is null)
        {
            return;
        }

        while (GetConnectionsTo(connector).Count > 0)
        {
            GetConnectionsTo(connector)[0].Break();
        }
    }

    protected override void OnAdd(IConnection newConnection)
    {
        RegisterConnectionWithConnector(newConnection.InputConnector, newConnection);
        RegisterConnectionWithConnector(newConnection.OutputConnector, newConnection);
        newConnection.OnBroken += Connection_OnBroken;
        base.OnAdd(newConnection);
    }

    protected override void OnRemove(IConnection removedConnection)
    {
        UnregisterConnectionWithConnector(removedConnection.InputConnector, removedConnection);
        UnregisterConnectionWithConnector(removedConnection.OutputConnector, removedConnection);
        removedConnection.OnBroken -= Connection_OnBroken;
        base.OnRemove(removedConnection);
    }

    private void Connection_OnBroken(object sender, EventArgs e)
    {
        _actionManager.ExecuteAction(new SeverConnectionAction((IConnection)sender, this));
    }

    private void UnregisterConnectionWithConnector(IIOConnector connector, IConnection removedConnection)
    {
        if (_connections[connector].Count == 1)
        {
            _connections.Remove(connector);
        }
        else
        {
            _connections[connector].Remove(removedConnection);
        }
    }

    private void RegisterConnectionWithConnector(IIOConnector connector, IConnection connection)
    {
        if (!_connections.ContainsKey(connector))
        {
            _connections.Add(connector, new List<IConnection>());
        }

        _connections[connector].Add(connection);
    }
}