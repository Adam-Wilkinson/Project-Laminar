using Laminar.Contracts.ActionSystem;
using Laminar.Contracts.NodeSystem.Connection;
using Laminar.Domain;

namespace Laminar.Core.ScriptEditor.Actions;

public class SeverConnectionAction : IUserAction
{
    readonly IConnection _connection;
    readonly IObservableCollection<IConnection> _connectionCollection;

    public SeverConnectionAction(IConnection connection, IObservableCollection<IConnection> connectionCollection)
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
