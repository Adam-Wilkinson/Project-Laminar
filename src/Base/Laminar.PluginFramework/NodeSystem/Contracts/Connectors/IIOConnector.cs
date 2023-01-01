using System;

namespace Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

public interface IIOConnector
{
    public PassUpdateOption PassUpdate(ExecutionFlags.ExecutionFlags executionFlags);

    public Action? PreEvaluateAction { get; }

    public string ColorHex { get; }
}
