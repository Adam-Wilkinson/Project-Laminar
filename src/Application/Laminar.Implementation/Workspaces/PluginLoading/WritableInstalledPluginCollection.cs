using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Workspaces;
using Laminar.Contracts.Workspaces.PluginLoading;
using Laminar.Domain.Extensions;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.Observables.Value;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Workspaces.PluginLoading;

public class WritableInstalledPluginCollection(IWorkspace hostWorkspace) : IWritableInstalledPluginCollection
{
    private readonly ObservableDictionary<string, IPluginInstallation> _userInstalledPluginsById = [];
    private readonly Dictionary<string, IObservableValue<SemanticVersion?>> _pluginVersionObservables = [];
    private InstalledPluginModificationScope ModificationScope => field ??= new(this, hostWorkspace);

    public IEnumerator<string> GetEnumerator() => _userInstalledPluginsById.Keys.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => _userInstalledPluginsById.Count;
    
    public event EventHandler<string>? ItemAdded
    {
        add => _userInstalledPluginsById.Keys.ItemAdded += value;
        remove => _userInstalledPluginsById.Keys.ItemAdded -= value;
    }
    
    public event EventHandler<string>? ItemRemoved    
    {
        add => _userInstalledPluginsById.Keys.ItemRemoved += value;
        remove => _userInstalledPluginsById.Keys.ItemRemoved -= value;
    }
    
    public event PluginChangedEventHandler? Changed
    {
        add => ModificationScope.Changed += value;
        remove => ModificationScope.Changed -= value;
    }
    
    public bool Contains(string value) => _userInstalledPluginsById.ContainsKey(value);
    
    public IReadOnlyObservableValue<SemanticVersion?> GetPluginVersionObservable(string pluginId)
    {
        if (_pluginVersionObservables.TryGetValue(pluginId, out var pluginVersionObservable))
        {
            return pluginVersionObservable;
        }

        var newObservable = new ObservableValue<SemanticVersion?>(
            _userInstalledPluginsById.TryGetValue(pluginId, out var installation) ? installation.PluginId.Version : null);
        _pluginVersionObservables.Add(pluginId, newObservable);
        return newObservable;
    }

    public SemanticVersion? GetPluginVersion(string pluginId) => TryGetInstallation(pluginId, out var installation)
        ? installation.PluginId.Version
        : null;

    public void Add(string pluginId, IPluginInstallation installation)
    {
        _userInstalledPluginsById.Add(pluginId, installation);

        if (_pluginVersionObservables.TryGetValue(pluginId, out var pluginVersionObservable))
        {
            pluginVersionObservable.Value = installation.PluginId.Version;
        }
    }

    public bool TryGetInstallation(string pluginId, [MaybeNullWhen(false)] out IPluginInstallation installation)
        => _userInstalledPluginsById.TryGetValue(pluginId, out installation);

    public void Remove(string pluginId)
    {
        _userInstalledPluginsById.Remove(pluginId);
        if (_pluginVersionObservables.TryGetValue(pluginId, out var pluginVersionObservable))
        {
            pluginVersionObservable.Value = null;
        }
    }

    public bool Remove(string pluginId, [MaybeNullWhen(false)] out IPluginInstallation installation)
    {
        var result = _userInstalledPluginsById.Remove(pluginId, out installation);
        if (_pluginVersionObservables.TryGetValue(pluginId, out var pluginVersionObservable))
        {
            pluginVersionObservable.Value = null;
        }
        return result;
    }

    public IDisposable CreateModificationScope() => ModificationScope.IncrementDepth();
    
    private class InstalledPluginModificationScope : IDisposable
    {
        private readonly Dictionary<string, PluginChangedType> _changes = [];
        private readonly IWorkspace _workspace;
        private int _depth;
        
        public InstalledPluginModificationScope(WritableInstalledPluginCollection parent, IWorkspace hostWorkspace)
        {
            parent.SubscribeForEach(OnPluginAdded, OnPluginRemoved);
            _workspace = hostWorkspace;
        }

        public InstalledPluginModificationScope IncrementDepth()
        {
            _depth++;
            return this;
        }
        
        public event PluginChangedEventHandler? Changed;

        private void OnPluginAdded(string plugin)
        {
            if (_depth == 0)
            {
                Changed?.Invoke(this, new PluginChangedEventArgs
                {
                    ChangedWorkspace = _workspace,
                    PluginId = plugin,
                    ChangeType = PluginChangedType.Added
                });
                return;
            }

            if (!_changes.TryAdd(plugin, PluginChangedType.Added))
            {
                _changes[plugin] = PluginChangedType.Changed;
            }
        }

        private void OnPluginRemoved(string plugin)
        {
            if (_depth == 0)
            {
                Changed?.Invoke(this, new PluginChangedEventArgs
                {
                    ChangedWorkspace = _workspace,
                    PluginId = plugin,
                    ChangeType = PluginChangedType.Removed
                });
                return;
            }

            if (!_changes.TryAdd(plugin, PluginChangedType.Removed))
            {
                _changes.Remove(plugin);
            }
        }
        
        public void Dispose()
        {
            if (_depth <= 0) 
            {
                _depth = 0;
                return;
            }
            
            _depth--;
            if (_depth != 0 || _changes.Count <= 0) return;
            
            foreach (var change in _changes)
            {
                Changed?.Invoke(this, new PluginChangedEventArgs
                {
                    ChangedWorkspace = _workspace,
                    PluginId = change.Key,
                    ChangeType = change.Value
                });
            }
            _changes.Clear();
        }
    }
}