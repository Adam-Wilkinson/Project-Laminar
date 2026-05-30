using Laminar.Contracts.Scripting.Connection;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Implementation.Scripting.Connections;

internal class Connection : IConnection, IEqualityComparer<IConnection>
{
    public required IInputConnector InputConnector { get; init; }

    public required IOutputConnector OutputConnector { get; init; }

    public bool Equals(IConnection? x, IConnection? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.InputConnector.Equals(y.InputConnector) && x.OutputConnector.Equals(y.OutputConnector);
    }

    public int GetHashCode(IConnection obj)
    {
        return HashCode.Combine(obj.InputConnector, obj.OutputConnector);
    }
}