using Laminar.Domain.Notification;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

namespace Laminar.Contracts.Scripting.Connection;

public interface IConnectionCollection : IObservableCollection<IConnection>
{
    public IReadOnlyList<IConnection> GetConnectionsTo(IIOConnector ioConnector);

    void RemoveConnectionsTo(IIOConnector connector);
}
