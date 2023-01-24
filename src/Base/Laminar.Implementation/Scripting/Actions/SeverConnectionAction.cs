using System.Collections.Generic;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.Connection;

namespace Laminar.Implementation.Scripting.Actions;

public class SeverConnectionAction : IUserAction
{
    readonly IConnection _connection;
    readonly ICollection<IConnection> _connectionCollection;

    public SeverConnectionAction(IConnection connection, ICollection<IConnection> connectionCollection)
    {
        _connection = connection;
        _connectionCollection = connectionCollection;
    }

    public bool Execute()
    {
        _connection.InputConnector.OnDisconnectedFrom(_connection.OutputConnector);
        _connection.OutputConnector.OnDisconnectedFrom(_connection.InputConnector);
        _connectionCollection.Remove(_connection);
        _connection.Break();
        return true;
    }

    public IUserAction GetInverse()
    {
        return new EstablishConnectionAction(_connection.OutputConnector, _connection.InputConnector, _connectionCollection);
    }
}
