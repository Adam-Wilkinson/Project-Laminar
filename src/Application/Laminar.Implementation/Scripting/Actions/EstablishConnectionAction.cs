using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Scripting.Connections;
using Laminar.PluginFramework.NodeSystem.Connectors;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Laminar.Implementation.Scripting.Actions;

public class EstablishConnectionAction(
    IOutputConnector connectorOne,
    IInputConnector connectorTwo,
    ICollection<IConnection> connectionCollection)
    : IUserAction
{
    private IConnection? _connection;

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute { get; } = connectorOne.CanConnectTo(connectorTwo) || connectorTwo.CanConnectTo(connectorOne);

    public Task<IUserActionResult> Execute()
    {
        if (!connectorOne.TryConnectTo(connectorTwo) && !connectorTwo.TryConnectTo(connectorOne)) 
            return Task.FromResult(IUserActionResult.Invalid());
        
        _connection = new Connection
        {
            OutputConnector = connectorOne,
            InputConnector = connectorTwo
        };
        connectionCollection.Add(_connection);

        return Task.FromResult(IUserActionResult.Success(new SeverConnectionAction(_connection, connectionCollection)));
    }
}
