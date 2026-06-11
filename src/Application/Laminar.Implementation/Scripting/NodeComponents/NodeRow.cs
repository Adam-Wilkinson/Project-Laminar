using System.Collections;
using Laminar.Domain.Extensions;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.NodeSystem.Connectors;
using Laminar.PluginFramework.NodeSystem.IO;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.Implementation.Scripting.NodeComponents;

internal class NodeRow(IInput? input, IOutput? output) : INodeRow
{
    public event EventHandler<LaminarExecutionContext>? StartExecution
    {
        add
        {
            if (input is not null)
            {
                input.ExecutionStarted += value;
            }

            if (output is not null)
            {
                output.ExecutionStarted += value;
            }

            if (CentralDisplay is ILaminarExecutionSource displaySource)
            {
                displaySource.ExecutionStarted += value;
            }
        }
        remove
        {
            if (input is not null)
            {
                input.ExecutionStarted -= value;
            }

            if (output is not null)
            {
                output.ExecutionStarted -= value;
            }

            if (CentralDisplay is ILaminarExecutionSource displaySource)
            {
                displaySource.ExecutionStarted -= value;
            }
        }
    }

    public required IInputConnector? InputConnector { get; init; }

    public required IOutputConnector? OutputConnector { get; init; }

    public required IInterfaceData CentralDisplay { get; init; }

    public Opacity Opacity { get; } = new();

    public IEnumerator<INodeComponent> GetEnumerator() => this.Yield().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
