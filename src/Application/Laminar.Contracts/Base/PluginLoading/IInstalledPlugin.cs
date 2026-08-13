using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IInstalledPlugin
{
    VersionedPluginId PluginId { get; }

    public IRuntimeHost Host { get; }
}