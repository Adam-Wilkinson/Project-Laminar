using System;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Contracts;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;
using Laminar.PluginFramework.NodeSystem.Contracts.IO;

namespace Laminar.Implementation.Scripting.NodeWrapping;

internal class NodeRow : INodeRow
{
    public NodeRow(IInput? input, IOutput? output)
    {
        if (input is not null)
        {
            input.StartExecution += Input_StartExecution;
        }

        if (output is not null)
        {
            output.StartExecution += Output_StartExecution;
        }
    }

    public event EventHandler<LaminarExecutionContext>? StartExecution;

    private void Output_StartExecution(object? sender, LaminarExecutionContext e) => StartExecution?.Invoke(sender, e with { ExecutionSource = OutputConnector });

    private void Input_StartExecution(object? sender, LaminarExecutionContext e) => StartExecution?.Invoke(sender, e with { ExecutionSource = InputConnector });

    public required IInputConnector? InputConnector { get; init; }

    public required IOutputConnector? OutputConnector { get; init; }

    public required object CentralDisplay { get; init; }

    public void CopyValueTo(INodeRow nodeRow)
    {
        if (CentralDisplay is IValueInfo copyFrom && nodeRow.CentralDisplay is IValueInfo copyTo)
        {
            copyTo.BoxedValue = copyFrom.BoxedValue;
        }
    }
}
