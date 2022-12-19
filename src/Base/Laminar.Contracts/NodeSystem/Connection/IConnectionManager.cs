using Laminar.Domain;

namespace Laminar.Contracts.NodeSystem.Connection;

internal interface IConnectionManager
{
    public IReadOnlyObservableCollection<IConnection> Connections { get; }

    public IDisposable? Connect(IConnectorView connectorOne, IConnectorView connectorTwo);

    public void SeverConnection(IConnection connection);

    void ClearConnector(IConnectorView connector);
}