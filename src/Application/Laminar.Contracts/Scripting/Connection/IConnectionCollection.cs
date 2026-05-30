using Laminar.Domain.Notification;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Contracts.Scripting.Connection;

public interface IConnectionCollection : IReadOnlyObservableCollection<IConnection>, IList<IConnection>
{
    public IReadOnlyList<IConnection> GetConnectionsTo(IConnector connector);

    void RemoveConnectionsTo(IConnector? connector);
}
