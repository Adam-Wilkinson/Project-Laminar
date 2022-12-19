using Laminar.Domain;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

namespace Laminar.Contracts.NodeSystem.Connection;

public interface IConnectionCollection : IObservableCollection<IConnection>
{
    public IReadOnlyList<IConnection> GetConnectionsTo(IIOConnector ioConnector);

    void RemoveConnectionsTo(IIOConnector connector);
}
