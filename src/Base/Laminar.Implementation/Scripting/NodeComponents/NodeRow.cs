using System;
using System.Collections;
using System.Collections.Generic;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Domain.Extensions;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.NodeSystem.Connectors;
using Laminar.PluginFramework.NodeSystem.IO;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.Implementation.Scripting.NodeComponents;

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

    public required IInputConnector? InputConnector { get; init; }

    public required IOutputConnector? OutputConnector { get; init; }

    public required object CentralDisplay { get; init; }

    public Opacity Opacity { get; } = new Opacity();

    public void CopyValueTo(INodeRow nodeRow)
    {
        if (CentralDisplay is IDisplay copyFrom && nodeRow.CentralDisplay is IDisplay copyTo)
        {
            copyTo.DisplayValue.Value = copyFrom.DisplayValue.Value;
        }
    }

    private void Output_StartExecution(object? sender, LaminarExecutionContext e) => StartExecution?.Invoke(sender, e with { ExecutionSource = OutputConnector });

    private void Input_StartExecution(object? sender, LaminarExecutionContext e) => StartExecution?.Invoke(sender, e with { ExecutionSource = InputConnector });

    public IEnumerator<INodeComponent> GetEnumerator() => this.Yield().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
