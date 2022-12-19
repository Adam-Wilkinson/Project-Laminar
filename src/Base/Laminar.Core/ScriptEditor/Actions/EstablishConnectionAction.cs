using System;
using Laminar.Contracts.ActionSystem;
using Laminar.Domain;
using Laminar.Core.ScriptEditor.Connections;
using Laminar.Contracts.NodeSystem.Connection;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

namespace Laminar.Core.ScriptEditor.Actions;

public class EstablishConnectionAction : IUserAction
{
    private readonly IOutputConnector _outputConnector;
    private readonly IInputConnector _inputConnector;
    private readonly IObservableCollection<IConnection> _connectionCollection;

    IConnection _connection;

    public EstablishConnectionAction(IOutputConnector connectorOne, IInputConnector connectorTwo, IObservableCollection<IConnection> connectionCollection)
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
