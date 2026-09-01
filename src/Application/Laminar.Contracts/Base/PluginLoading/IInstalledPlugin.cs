using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IInstalledPlugin
{
    VersionedPluginId PluginId { get; }

    public IRuntimeHost Host { get; }

    public bool TryGetNodeInfo(string nodeName, [NotNullWhen(true)] out ILoadedNodeInfo? nodeInfo);
}