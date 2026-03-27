using System;
using System.Collections.Generic;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Scripting.Actions;

public class SeverConnectionAction(IConnection connection, ICollection<IConnection> connectionCollection)
    : IUserAction
{
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute { get; } = true;

    public UserActionResult Execute()
    {
        connection.InputConnector.OnDisconnectedFrom(connection.OutputConnector);
        connection.OutputConnector.OnDisconnectedFrom(connection.InputConnector);
        connectionCollection.Remove(connection);
        connection.Break();
        return UserActionResult.Success(new EstablishConnectionAction(connection.OutputConnector, connection.InputConnector,
            connectionCollection));
    }
}
