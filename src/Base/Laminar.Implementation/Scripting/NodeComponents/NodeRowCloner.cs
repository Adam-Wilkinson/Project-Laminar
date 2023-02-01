using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Contracts.Components;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.Implementation.Scripting.NodeComponents;

internal class NodeRowCloner<T> : INodeComponentCloner<T> where T : INodeComponent
{
    private readonly Func<T> _generator;

    private readonly List<T> _coreList = new();
    private readonly ObservableCollection<T> _components = new();

    public NodeRowCloner(Func<T> generator, int startCount)
    {
        _generator = generator;

        for (int i = 0; i < startCount + 1; i++)
        {
            AddNewRow();
        }
    }

    public event EventHandler<LaminarExecutionContext>? StartExecution;

    public event NotifyCollectionChangedEventHandler? CollectionChanged
    {
        add => _components.CollectionChanged += value;
        remove => _components.CollectionChanged -= value;
    }

    public Opacity Opacity { get; } = new();

    public int Count => _components.Count;

    public IEnumerator<T> GetEnumerator() => _components.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void RemoveRowAt(int index)
    {
        _coreList.RemoveAt(index);
        _components.RemoveAt(index);
    }

    private void LeadingRow_StartExecution(object? sender, LaminarExecutionContext e) => AddNewRow();

    private void AddNewRow()
    {
        if (_components.Count > 0)
        {
            _components[^1].StartExecution -= LeadingRow_StartExecution;
            _components[^1].Opacity.SetInternalValue(1.0);
        }

        T leadingItem = _generator();
        leadingItem.Opacity.AddFactor(Opacity);
        leadingItem.Opacity.SetInternalValue(0.5);
        leadingItem.StartExecution += LeadingRow_StartExecution;
        leadingItem.StartExecution += StartExecution;

        _components.Add(leadingItem);
    }

    IEnumerator<INodeComponent> IEnumerable<INodeComponent>.GetEnumerator()
    {
        foreach (T component in _components)
        {
            yield return component;
        }
    }
}
