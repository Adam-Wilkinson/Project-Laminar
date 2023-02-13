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
    private readonly IInput? _input;
    private readonly IOutput? _output;

    public NodeRow(IInput? input, IOutput? output)
    {
        _input = input;
        _output = output;
    }

    public event EventHandler<LaminarExecutionContext>? StartExecution
    {
        add
        {
            if (_input is not null)
            {
                _input.ExecutionStarted += value;
            }

            if (_output is not null)
            {
                _output.ExecutionStarted += value;
            }
        }
        remove
        {
            if (_input is not null)
            {
                _input.ExecutionStarted -= value;
            }

            if (_output is not null)
            {
                _output.ExecutionStarted -= value;
            }
        }
    }

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

    public IEnumerator<INodeComponent> GetEnumerator() => this.Yield().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
