namespace Laminar.PluginFramework.NodeSystem.Connectors;

[Flags]
public enum ConnectorFlags
{
    None = 0,
    AcceptsConnections = 1,
    HasConnections = 2,
    ConnectionsSaturated = 4,
}