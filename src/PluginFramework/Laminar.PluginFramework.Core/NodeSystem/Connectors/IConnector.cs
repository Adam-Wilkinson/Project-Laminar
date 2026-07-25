using System.ComponentModel;

namespace Laminar.PluginFramework.NodeSystem.Connectors;

public interface IConnector : INotifyPropertyChanged
{
    public Action? PreEvaluateAction { get; }

    public ConnectorFlags Flags { get; }
    
    public string ColorHex { get; }
}
