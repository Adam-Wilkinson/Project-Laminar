using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.Connection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Laminar.Implementation.Scripting.Actions;

public class SeverConnectionAction(IConnection connection, ICollection<IConnection> connectionCollection)
    : IUserAction
{
    public event EventHandler? CanExecuteChanged { add { } remove { } }

    public bool CanExecute { get; } = true;

    public Task<IUserActionResult> Execute()
    {
        connection.InputConnector.OnConnectionSevered();
        connection.OutputConnector.OnConnectionSevered();
        connectionCollection.Remove(connection);
        connection.Break();
        return Task.FromResult(IUserActionResult.Success(new EstablishConnectionAction(connection.OutputConnector, connection.InputConnector,
            connectionCollection)));
    }
}
