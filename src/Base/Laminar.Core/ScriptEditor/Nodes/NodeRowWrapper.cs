using System.ComponentModel;
using System;
using Laminar.Contracts.NodeSystem;
using System.Diagnostics;
using Laminar_PluginFramework.NodeSystem;
using Laminar.Contracts.Primitives;
using Laminar.Contracts.UserInterface;
using Laminar.PluginFramework.NodeSystem;
using Laminar.Contracts.NodeSystem.Connection;

namespace Laminar.Core.ScriptEditor.Nodes;

public class NodeRowWrapper : INodeRowWrapper
{
    readonly INotificationClient<LaminarExecutionContext> _userChangedValueNotifiee;

    public NodeRowWrapper(NodeRow row, IConnectorViewFactory connectorFactory, IValueDisplayFactory valueDisplayFactory, INotificationClient<LaminarExecutionContext> userChangedValueNotifiee)
    {
        if (row.Input is not null)
        {
            row.Input.StartExecution += StartExecution;
            InputConnector = connectorFactory.CreateConnector(row.Input);
        }

        if (row.Output is not null)
        {
            row.Output.StartExecution += StartExecution;
            OutputConnector = connectorFactory.CreateConnector(row.Output);
        }

        Display = valueDisplayFactory.CreateValueDisplay(row.DisplayValue);
        _userChangedValueNotifiee = userChangedValueNotifiee;
        RefreshDisplay();
    }

    private void StartExecution(object sender, LaminarExecutionContext e)
    {
        _userChangedValueNotifiee.TriggerNotification(e);
    }

    public IConnectorView? InputConnector { get; }

    public IConnectorView? OutputConnector { get; }

    public IValueDisplay Display { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void CloneTo(INodeRowWrapper cloneTo)
    {
        Display.CopyValueTo(cloneTo.Display);
    }

    public void RefreshDisplay()
    {
        InputConnector?.Refresh();
        OutputConnector?.Refresh();
        Display.Refresh();
    }
}