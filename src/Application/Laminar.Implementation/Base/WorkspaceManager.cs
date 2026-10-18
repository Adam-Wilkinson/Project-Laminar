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

    public IReadOnlyObservableList<IWorkspace> Workspaces => _hosts;

    public IWorkspace CreateWorkspace(string name)
    {
        var newHost = new Workspace(serviceProvider, actionManager)
        {
            Name = name
        };
        
        _hosts.Add(newHost);
        newHost.PluginManager.PluginChanged += PluginManagerOnPluginChanged;
        return newHost;
    }

    private void PluginManagerOnPluginChanged(object? sender, PluginChangedEventArgs e)
    {
        PluginChanged?.Invoke(sender, e);
    }

    public event PluginChangedEventHandler? PluginChanged;

    public void Dispose()
    {
        foreach (var  host in _hosts)
            host.PluginManager.PluginChanged -= PluginManagerOnPluginChanged;
    }
}