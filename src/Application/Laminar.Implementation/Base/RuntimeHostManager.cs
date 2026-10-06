using Laminar.Contracts.Base;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Base;

public class RuntimeHostManager(IServiceProvider serviceProvider, IUserActionManager actionManager) : IRuntimeHostManager
{
    private readonly List<IRuntimeHost> _hosts = [];

    public IReadOnlyCollection<IRuntimeHost> AllHosts => _hosts;

    public IRuntimeHost CreateRuntimeHost(string name)
    {
        var newHost = new RuntimeHost(serviceProvider, actionManager)
        {
            Name = name
        };
        
        _hosts.Add(newHost);
        newHost.PluginManager.UserInstalledPlugins.ItemAdded += HostPluginChanged;
        newHost.PluginManager.UserInstalledPlugins.ItemRemoved += HostPluginChanged;
        return newHost;

        void HostPluginChanged(object? sender, VersionedPluginId plugin)
        {
            PluginsChanged?.Invoke(this, new PluginsChangedEventArgs
            {
                ChangedRuntime = newHost,
            });
        }
    }

    public event EventHandler<PluginsChangedEventArgs>? PluginsChanged;
}