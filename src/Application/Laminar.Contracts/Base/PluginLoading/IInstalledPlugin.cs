using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IInstalledPlugin
{
    public VersionedPluginId Id { get; }
}