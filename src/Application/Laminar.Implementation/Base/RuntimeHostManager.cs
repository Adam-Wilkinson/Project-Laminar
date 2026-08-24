using System.Collections.Specialized;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;

namespace Laminar.Implementation.Base;

public class RuntimeHostManager(IServiceProvider serviceProvider) : IRuntimeHostManager
{
    private readonly List<IRuntimeHost> _hosts = [];

    public IReadOnlyCollection<IRuntimeHost> AllHosts => _hosts;

    public IRuntimeHost CreateRuntimeHost(string name)
    {
        var newHost = new RuntimeHost(serviceProvider)
        {
            Name = name
        };
        
        _hosts.Add(newHost);
        newHost.PluginManager.Plugins.CollectionChanged += (_, _) => PluginsChanged?.Invoke(this,
            new PluginsChangedEventArgs
            {
                ChangedRuntime = newHost,
            });
        return newHost;
    }

    public event EventHandler<PluginsChangedEventArgs>? PluginsChanged;
}