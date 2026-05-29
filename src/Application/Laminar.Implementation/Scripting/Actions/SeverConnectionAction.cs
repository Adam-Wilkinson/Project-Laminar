using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.Connection;

namespace Laminar.Implementation.Scripting.Actions;

internal readonly struct SeverConnectionAction(IConnection connection, ICollection<IConnection> connectionCollection)
    : IUserAction
{
    public IConnection Connection { get; } = connection;

    public bool CanExecute => connectionCollection.Contains(Connection);

    public Task<IUserActionResult> Execute()
    {
        if (!connectionCollection.Contains(Connection))
        {
            return Task.FromResult(IUserActionResult.Invalid());
        }
        
        Connection.InputConnector.OnConnectionSevered();
        Connection.OutputConnector.OnConnectionSevered();
        connectionCollection.Remove(Connection);
        Connection.Break();
        return Task.FromResult(IUserActionResult.Success(
            new EstablishConnectionAction(Connection.OutputConnector, Connection.InputConnector,
            connectionCollection)));
    }

    public IUserActionSimplification GetSimplificationAfter(IUserAction previousAction)
    {
        if (previousAction is EstablishConnectionAction establishAction &&
            Equals(establishAction.OutputConnector, Connection.OutputConnector) &&
            Equals(establishAction.InputConnector, Connection.InputConnector))
        {
            return IUserActionSimplification.Undoes();
        }

        return IUserActionSimplification.None();
    }
}
