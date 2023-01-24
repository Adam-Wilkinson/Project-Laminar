using System;
using System.Collections.Generic;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Implementation.Scripting.Connections;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

namespace Laminar.Implementation.Scripting.Actions;

public class EstablishConnectionAction : IUserAction
{
    private readonly IOutputConnector _outputConnector;
    private readonly IInputConnector _inputConnector;
    private readonly ICollection<IConnection> _connectionCollection;

    IConnection? _connection;

    public EstablishConnectionAction(IOutputConnector connectorOne, IInputConnector connectorTwo, ICollection<IConnection> connectionCollection)
    {
        _outputConnector = connectorOne;
        _inputConnector = connectorTwo;
        _connectionCollection = connectionCollection;
    }

    public bool Execute()
    {
        if (_outputConnector.TryConnectTo(_inputConnector) || _inputConnector.TryConnectTo(_outputConnector))
        {
            _connection = new Connection
            {
                OutputConnector = _outputConnector,
                InputConnector = _inputConnector
            };
            _connectionCollection.Add(_connection);
            return true;
        }

        return false;
    }

    public IUserAction GetInverse()
    {
        if (_connection is null)
        {
            throw new InvalidOperationException("Cannot get the sever action for a connection that hasn't been made");
        }

        return new SeverConnectionAction(_connection, _connectionCollection);
    }
}
