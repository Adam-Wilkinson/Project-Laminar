using System;
using System.ComponentModel;

namespace Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

public interface IIOConnector : INotifyPropertyChanged
{
    public Action? PreEvaluateAction { get; }

    public string ColorHex { get; }
}
