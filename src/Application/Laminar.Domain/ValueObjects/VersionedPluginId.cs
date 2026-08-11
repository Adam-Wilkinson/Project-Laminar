namespace Laminar.Domain.ValueObjects;

public record struct VersionedPluginId(string Name, SemanticVersion Version);