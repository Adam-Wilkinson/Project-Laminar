using System;

namespace Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

public interface IIOConnector
{
    public ActivitySetting ActivitySetting { get; }

    public Action? PreEvaluateAction { get; }

    public string ColorHex { get; }
}
