using System.ComponentModel;
using Laminar.Contracts.Base.UserInterface;
using Laminar.Contracts.Primitives;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Contracts.IO;

namespace Laminar.Implementation.Scripting.Nodes;

public class WrappedNodeRow : IWrappedNodeRow
{
    readonly INotificationClient<LaminarExecutionContext> _userChangedValueNotifiee;

    public WrappedNodeRow(NodeRow row, IConnectorViewFactory connectorFactory, IDisplayFactory valueDisplayFactory, INotificationClient<LaminarExecutionContext> userChangedValueNotifiee)
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

        Display = valueDisplayFactory.CreateDisplayForValue(row.DisplayValue);
        _userChangedValueNotifiee = userChangedValueNotifiee;
    }

    private void StartExecution(object sender, LaminarExecutionContext e)
    {
        if (e.ExecutionSource is IOutput)
        {
            _userChangedValueNotifiee.TriggerNotification(e with { ExecutionSource = OutputConnector.NodeIOConnector });
        }
        else if (e.ExecutionSource is IInput)
        {
            _userChangedValueNotifiee.TriggerNotification(e with { ExecutionSource = InputConnector.NodeIOConnector });
        }
        _userChangedValueNotifiee.TriggerNotification(e);
    }

    public IConnectorView? InputConnector { get; }

    public IConnectorView? OutputConnector { get; }

    public IDisplay Display { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void CloneTo(IWrappedNodeRow cloneTo)
    {
        cloneTo.Display.Value.Value = Display.Value.Value;
    }

    public void RefreshDisplay()
    {
        InputConnector?.Refresh();
        OutputConnector?.Refresh();
        Display.Refresh();
    }
}