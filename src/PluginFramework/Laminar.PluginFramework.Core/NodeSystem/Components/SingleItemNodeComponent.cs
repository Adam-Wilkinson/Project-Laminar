using System;
using System.Collections;
using System.Collections.Generic;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.PluginFramework.NodeSystem.Components;

public abstract class SingleItemNodeComponent : INodeComponent
{
    private INodeComponent? _component;

    protected INodeComponent? ChildComponent
    {
        get => _component;
        set
        {
            if (value is null)
            {
                return;
            }

            _component = value;
            _component.Opacity.AddFactor(Opacity);
            _component.StartExecution += (o, e) => StartExecution?.Invoke(o, e);
        }
    }

    public Opacity Opacity { get; } = new();

    public event EventHandler<LaminarExecutionContext>? StartExecution;

    public IEnumerator<INodeComponent> GetEnumerator()
    {
        yield return _component!;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
