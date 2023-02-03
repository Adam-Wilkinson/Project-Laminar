using System.Collections.Specialized;
using Laminar.Domain.Notification;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Contracts.Scripting.Connection;

public interface IConnectionCollection : IReadOnlyObservableCollection<IConnection>, IList<IConnection>
{
    public IReadOnlyList<IConnection> GetConnectionsTo(IIOConnector ioConnector);

    void RemoveConnectionsTo(IIOConnector? connector);
}
