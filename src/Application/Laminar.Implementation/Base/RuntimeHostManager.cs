using Laminar.Contracts.Base;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Base;

internal sealed class RuntimeHostManager(IServiceProvider serviceProvider, IUserActionManager actionManager) 
    : IRuntimeHostManager, IDisposable
{
    private readonly List<IRuntimeHost> _hosts = [];
    private readonly CompositeDisposable _disposables = new();

    public IReadOnlyCollection<IRuntimeHost> AllHosts => _hosts;

    public IRuntimeHost CreateRuntimeHost(string name)
    {
        var newHost = new RuntimeHost(serviceProvider, actionManager)
        {
            Name = name
        };
        
        _hosts.Add(newHost);
        _disposables.Add(newHost.PluginManager.UserInstalledPlugins.SubscribeForEach(HostPluginAdded, HostPluginRemoved));

        return newHost;

        void HostPluginAdded(string plugin)
        {
            PluginsChanged?.Invoke(this, new PluginsChangedEventArgs
            {
                ChangedRuntime = newHost,
                PluginId = plugin,
                ChangeType = PluginChangedType.Added
            });
        }
        
        void HostPluginRemoved(string plugin)
        {
            PluginsChanged?.Invoke(this, new PluginsChangedEventArgs
            {
                ChangedRuntime = newHost,
                PluginId = plugin,
                ChangeType = PluginChangedType.Removed
            });
        }
    }

    public event EventHandler<PluginsChangedEventArgs>? PluginsChanged;

    public void Dispose()
    {
        _disposables.Dispose();
    }
}