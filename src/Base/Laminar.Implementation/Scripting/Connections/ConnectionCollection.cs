using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Implementation.Scripting.Actions;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

namespace Laminar.Implementation.Scripting.Connections;

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
        return !_connections.ContainsKey(ioConnector) ? Array.Empty<IConnection>() : _connections[ioConnector];
    }

    public void RemoveConnectionsTo(IIOConnector? connector)
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

    protected override void InsertItem(int index, IConnection newConnection)
    {
        ItemAdded(newConnection);
        base.InsertItem(index, newConnection);
    }

    protected override void RemoveItem(int index)
    {
        ItemRemoved(this[index]);
        base.RemoveItem(index);
    }

    protected override void SetItem(int index, IConnection item)
    {
        ItemRemoved(this[index]);
        base.SetItem(index, item);
        ItemAdded(item);
    }

    private void ItemRemoved(IConnection removedConnection)
    {
        UnregisterConnectionWithConnector(removedConnection.InputConnector, removedConnection);
        UnregisterConnectionWithConnector(removedConnection.OutputConnector, removedConnection);
        removedConnection.OnBroken -= Connection_OnBroken;
    }

    private void ItemAdded(IConnection newConnection)
    {
        RegisterConnectionWithConnector(newConnection.InputConnector, newConnection);
        RegisterConnectionWithConnector(newConnection.OutputConnector, newConnection);
        newConnection.OnBroken += Connection_OnBroken;
    }

    private void Connection_OnBroken(object? sender, EventArgs e)
    {
        ArgumentNullException.ThrowIfNull(sender);

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