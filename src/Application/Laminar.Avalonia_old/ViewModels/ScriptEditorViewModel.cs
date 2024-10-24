using System.Collections.Generic;
using System;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain;
using ReactiveUI;
using System.Diagnostics;
using Avalonia.Controls;
using Laminar.Avalonia.Controls;

namespace Laminar.Avalonia.ViewModels;

public class ScriptEditorViewModel : ViewModelBase
{
    readonly IScript _script;
    readonly ILoadedNodeManager _loadedNodeManager;

    private string _name;

    public ScriptEditorViewModel(IScript script, ILoadedNodeManager loadedNodeManager)
    {
        _loadedNodeManager = loadedNodeManager;
        _script = script;

        Name = script.Name;
        AllLoadedNodes = _loadedNodeManager.LoadedNodes.SubCatagories;
        this.ObservableForProperty(x => x.Name).Subscribe(x => { _script.Name = x.Value; });
    }

    public IReadOnlyList<ItemCatagory<IWrappedNode>> AllLoadedNodes { get; }

    public void AddNode(IWrappedNode node)
    {
        Debug.WriteLine($"A node was added called {node}");
    }

    public Func<IControl, IControl> DuplicateNodePicker { get; } = (control) => new NodePicker();

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }
}
