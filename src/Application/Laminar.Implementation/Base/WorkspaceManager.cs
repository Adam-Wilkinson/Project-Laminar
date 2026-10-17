using Laminar.Contracts.Base;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Base;

internal sealed class WorkspaceManager(IServiceProvider serviceProvider, IUserActionManager actionManager) 
    : IWorkspaceManager, IDisposable
{
    private readonly ObservableList<IWorkspace> _hosts = [];
    private readonly CompositeDisposable _disposables = new();

    public IReadOnlyObservableList<IWorkspace> Workspaces => _hosts;

    public IWorkspace CreateWorkspace(string name)
    {
        var newHost = new Workspace(serviceProvider, actionManager)
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
                ChangedWorkspace = newHost,
                PluginId = plugin,
                ChangeType = PluginChangedType.Added
            });
        }
        
        void HostPluginRemoved(string plugin)
        {
            PluginsChanged?.Invoke(this, new PluginsChangedEventArgs
            {
                ChangedWorkspace = newHost,
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