using System;

namespace Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

public interface IIOConnector
{
    public Action? PreEvaluateAction { get; }

    public string ColorHex { get; }
}
