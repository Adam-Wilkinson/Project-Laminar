using System;
using System.ComponentModel;

namespace Laminar.PluginFramework.NodeSystem.Connectors;

public interface IIOConnector : INotifyPropertyChanged
{
    public Action? PreEvaluateAction { get; }

    public string ColorHex { get; }
}
