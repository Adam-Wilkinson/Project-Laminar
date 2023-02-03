using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Shapes;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Avalonia.NodeDisplaySystem;

public class ConnectorControl : Path
{
    private readonly static Dictionary<IIOConnector, ConnectorControl> _connectors = new();

    public static ConnectorControl FromConnector(IIOConnector connector) => _connectors[connector];

    public static readonly StyledProperty<IIOConnector> ConnectorProperty = AvaloniaProperty.Register<ConnectorControl, IIOConnector>(nameof(Connector));

    public IIOConnector Connector
    {
        get => GetValue(ConnectorProperty);
        set
        {
            _connectors[Connector] = this;
            SetValue(ConnectorProperty, value);
        }
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _connectors[Connector] = this;
    }
}
