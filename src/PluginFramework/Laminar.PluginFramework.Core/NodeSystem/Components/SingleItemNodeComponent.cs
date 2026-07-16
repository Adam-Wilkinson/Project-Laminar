using System.Collections;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.PluginFramework.NodeSystem.Components;

public abstract class SingleItemNodeComponent<TComponent> : INodeComponent where TComponent : INodeComponent
{
    protected TComponent? ChildComponent
    {
        get;
        set
        {
            field?.StartExecution -= OnFieldStartExecution;
            field?.Opacity.RemoveFactor(Opacity);
            field = value;
            field?.Opacity.AddFactor(Opacity);
            field?.StartExecution += OnFieldStartExecution;
        }
    }

    private void OnFieldStartExecution(object? sender, LaminarExecutionContext e)
    {
        StartExecution?.Invoke(this, e);
    }

    public Opacity Opacity { get; } = new();

    public event EventHandler<LaminarExecutionContext>? StartExecution;

    public IEnumerator<INodeComponent> GetEnumerator()
    {
        if (ChildComponent is null)
        {
            yield break;
        }
        
        yield return ChildComponent;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
