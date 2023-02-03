using System;
using Laminar.Contracts.Scripting.Connection;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Implementation.Scripting.Connections;

internal class Connection : IConnection
{
    public required IInputConnector InputConnector { get; init; }

    public required IOutputConnector OutputConnector { get; init; }

    public event EventHandler? OnBroken;

    public void Break()
    {
        OnBroken?.Invoke(this, EventArgs.Empty);
    }
}