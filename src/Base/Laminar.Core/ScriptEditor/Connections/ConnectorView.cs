using System;
using System.Collections.Generic;
using System.ComponentModel;
using Laminar.Contracts.NodeSystem.Connection;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;
using Laminar.PluginFramework.NodeSystem.Contracts.IO;

namespace Laminar.Core.ScriptEditor.Connections;

internal class ConnectorView : IConnectorView
{
    public ConnectorView(IIOConnector ioConnector)
    {
        NodeIOConnector = ioConnector;
        Refresh();
    }

    public string ColourHex { get; private set; }

    public IIOConnector NodeIOConnector { get; }

    public event PropertyChangedEventHandler PropertyChanged;

    public void Refresh()
    {
        if (ColourHex != NodeIOConnector.ColorHex)
        {
            ColourHex = NodeIOConnector.ColorHex;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ColourHex)));
        }
    }
}
