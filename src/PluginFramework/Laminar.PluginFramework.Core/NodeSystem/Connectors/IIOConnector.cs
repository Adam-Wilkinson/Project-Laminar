using System.ComponentModel;

namespace Laminar.PluginFramework.NodeSystem.Connectors;

public interface IIOConnector : INotifyPropertyChanged
{
    public Action? PreEvaluateAction { get; }

    public bool AcceptsConnections { get; }

    public void OnConnectionEstablished();

    public void OnConnectionSevered();
    
    public string ColorHex { get; }
}
