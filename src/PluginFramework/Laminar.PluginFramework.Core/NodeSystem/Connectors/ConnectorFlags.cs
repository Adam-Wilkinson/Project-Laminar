namespace Laminar.PluginFramework.NodeSystem.Connectors;

[Flags]
public enum ConnectorFlags
{
    AcceptsConnections = 0,
    HasConnections = 1,
    ConnectionsSaturated = 2,
}