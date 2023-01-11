using System.ComponentModel;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

namespace Laminar.Contracts.Scripting.Connection;

public interface IConnectorView : INotifyPropertyChanged
{
    public string ColourHex { get; }

    public IIOConnector NodeIOConnector { get; }

    public void Refresh();
}

public enum ConnectorType
{
    Input,
    Output,
}
