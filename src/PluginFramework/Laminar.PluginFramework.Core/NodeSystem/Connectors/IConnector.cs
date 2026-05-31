using System.ComponentModel;

namespace Laminar.PluginFramework.NodeSystem.Connectors;

public interface IConnector : INotifyPropertyChanged
{
    public Action? PreEvaluateAction { get; }

    public ConnectorStatus Status { get; }

    public void OnConnectionEstablished();

    public void OnConnectionSevered();
    
    public string ColorHex { get; }
}
