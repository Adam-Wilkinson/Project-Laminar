using System.Collections.Generic;
using System.Reactive.Linq;
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
using Laminar.Avalonia.NodeDisplaySystem;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Domain.Notification;

namespace Laminar.Avalonia.Controls.ScriptEditor;

internal class ConnectionCanvas : Canvas
{
    public static readonly StyledProperty<IReadOnlyObservableCollection<IConnection>> ConnectionsProperty = AvaloniaProperty.Register<ConnectionCanvas, IReadOnlyObservableCollection<IConnection>>(nameof(Connections));

    private readonly Dictionary<IConnection, ConnectionGeometry> _connections = new();

    public IReadOnlyObservableCollection<IConnection> Connections
    {
        get => GetValue(ConnectionsProperty);
        set
        {
            if (Connections is not null)
            {
                Connections.ItemAdded -= ConnectionManager_ConnectionEstablished;
                Connections.ItemRemoved -= ConnectionManager_ConnectionLost;
                _connections.Clear();
            }

            SetValue(ConnectionsProperty, value);
            if (Connections is not null)
            {
                Connections.ItemAdded += ConnectionManager_ConnectionEstablished;
                Connections.ItemRemoved += ConnectionManager_ConnectionLost;
                foreach (IConnection connection in Connections)
                {
                    ConnectionManager_ConnectionEstablished(Connections, connection);
                }
            }

            InvalidateVisual();
        }
    }

    public IEnumerable<ConnectionGeometry> ConnectionGeometries => _connections.Values;

    public override void Render(DrawingContext context)
    {
        foreach (ConnectionGeometry connection in _connections.Values)
        {
            if (connection.GetValue(Selection.SelectedProperty))
            {
                context.DrawGeometry(null, new Pen(Brushes.White, connection.Pen.Thickness * 1.7), connection);
            }
            
            context.DrawGeometry(null, connection.Pen, connection);
        }
        base.Render(context);
    }

    public void DeleteSelection(SelectionManager selectionManager)
    {
        foreach (ConnectionGeometry connection in selectionManager.GetSelection<ConnectionGeometry>())
        {
            connection.CoreConnection.Break();
        }
    }

    private void ConnectionManager_ConnectionLost(object sender, IConnection e)
    {
        _connections[e].Changed -= ConnectionGeometryChanged;
        _connections.Remove(e);
        InvalidateVisual();
    }

    private void ConnectionManager_ConnectionEstablished(object sender, IConnection e)
    {
        ConnectionGeometry newConnection = new() { CoreConnection = e, Pen = new Pen(new SolidColorBrush(Color.Parse(e.OutputConnector.ColorHex)), 3) };
        _connections.Add(e, newConnection);

        Control inputControl = ConnectorControl.FromConnector(e.InputConnector);
        newConnection[!ConnectionGeometry.EndPointProperty] = inputControl.GetObservable(TransformedBoundsProperty).Select(GetTransformBoundsCenter).ToBinding();

        Control outputControl = ConnectorControl.FromConnector(e.OutputConnector);
        newConnection[!ConnectionGeometry.StartPointProperty] = outputControl.GetObservable(TransformedBoundsProperty).Select(GetTransformBoundsCenter).ToBinding();

        newConnection.Changed += ConnectionGeometryChanged;

        InvalidateVisual();
    }

    private void ConnectionGeometryChanged(object sender, EventArgs e) => InvalidateVisual();

    private Point GetTransformBoundsCenter(TransformedBounds? rect)
        => rect.Value.Bounds.Center.Transform(rect.Value.Transform * this.TransformedBounds.Value.Transform.Invert());// visual.VisualParent.TranslatePoint(visual.Bounds.Center, this).Value;
}
