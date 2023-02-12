using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Shapes;
using Laminar.PluginFramework.NodeSystem.Connectors;
using Laminar.Domain.Observable;

namespace Laminar.Avalonia.NodeDisplaySystem;

public class ConnectorControl : Path
{
    private readonly static Dictionary<IIOConnector, Observable<ConnectorControl>> _connectorObservables = new();

    public static ConnectorControl FromConnector(IIOConnector connector) => _connectorObservables[connector].LastValue!;

    public static IObservable<ConnectorControl> FromConnectorObservable(IIOConnector connector) 
    {
        if (_connectorObservables.TryGetValue(connector, out Observable<ConnectorControl> value))
        {
            return value;
        }

        Observable<ConnectorControl> newObservable = new();
        _connectorObservables.Add(connector, newObservable);
        return newObservable;
    }

    public static readonly StyledProperty<IIOConnector> ConnectorProperty = AvaloniaProperty.Register<ConnectorControl, IIOConnector>(nameof(Connector));

    public ConnectorControl()
    {
        this.GetObservable(ConnectorProperty).Subscribe(connector =>
        {
            if (connector is not null)
            {
                SetConnector(connector);
            }
        });
    }

    public IIOConnector Connector
    {
        get => GetValue(ConnectorProperty);
        set => SetValue(ConnectorProperty, value);
    }

    private void SetConnector(IIOConnector connector)
    {
        if (!_connectorObservables.ContainsKey(connector))
        {
            _connectorObservables[connector] = new();
        }

        _connectorObservables[connector].ChangeValue(this);
    }
}
