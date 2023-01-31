using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Laminar.Contracts.Base.UserInterface;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Contracts;

namespace Laminar.Implementation.Scripting.NodeWrapping;

internal class NodeRowCloner<T> : INodeRowCloner<T> where T : IConvertsToNodeRow
{
    private readonly Func<T> _generator;
    private readonly INodeRowFactory _rowFactory;

    private readonly List<T> _coreList = new();
    private readonly ObservableCollection<INodeRow> _components = new();

    private NodeComponent? _component;

    public NodeRowCloner(INodeRowFactory rowFactory, Func<T> generator, int startCount)
    {
        _generator = generator;
        _rowFactory = rowFactory;

        for (int i = 0; i < startCount + 1; i++)
        {
            AddNewRow();
        }
    }

    public T this[int index] => _coreList[index];

    public int Count => _coreList.Count;

    public NodeComponent GetComponent() => _component ??= NodeComponent.Collection(_components);

    public IEnumerator<T> GetEnumerator() => _coreList.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void RemoveRowAt(int index)
    {
        _coreList.RemoveAt(index);
        _components.RemoveAt(index);
    }

    private void LeadingRow_StartExecution(object? sender, LaminarExecutionContext e)
    {
        AddNewRow();
    }

    private void AddNewRow()
    {
        if (_components.Count > 0)
        {
            _components[^1].StartExecution -= LeadingRow_StartExecution;
            (_components[^1].CentralDisplay as IDisplay)?.Opacity.SetInternalValue(1.0);
        }

        T leadingItem = _generator();
        _coreList.Add(leadingItem);
        INodeRow leadingRow = leadingItem.GetRow();
        if (leadingRow.CentralDisplay is IDisplay display)
        {
            display.Opacity.SetInternalValue(0.5);
        }

        leadingRow.StartExecution += LeadingRow_StartExecution;
        _components.Add(leadingRow);
    }
}
