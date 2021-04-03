namespace OpenFlow_Avalonia.NodeDisplaySystem
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Controls.Shapes;
    using Avalonia.Input;
    using Avalonia.Media;
    using Avalonia.VisualTree;
    using OpenFlow_Core.Nodes;
    using OpenFlow_Core.Nodes.Connectors;
    using OpenFlow_Core.Nodes.NodeTreeSystem;

    public class NodeDisplayCanvas : Canvas
    {
        private readonly Pen pen = new(Brushes.LightGray);
        private readonly NodeTree manager = new();
        private List<NodeDisplay> selectedNodes = new();
        private Point originalClickPoint;
        private DragType dragType = DragType.None;
        private Control selectedField;
        private Point currentMousePos;

        public NodeDisplayCanvas()
        {
            foreach (INodeBase node in manager.Nodes)
            {
                NodeAdded(node);
            }

            manager.Nodes.CollectionChanged += Nodes_CollectionChanged;
        }

        private enum DragType
        {
            SelectionBox,
            MovingNodes,
            CreatingCurve,
            None,
        }

        public double FlatSize { get; set; } = 1;

        private Rect SelectionRect => new(
            Math.Min(originalClickPoint.X, currentMousePos.X),
            Math.Min(originalClickPoint.Y, currentMousePos.Y),
            Math.Abs(originalClickPoint.X - currentMousePos.X),
            Math.Abs(originalClickPoint.Y - currentMousePos.Y));

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            pen.Thickness = FlatSize * 5;
            if (dragType == DragType.SelectionBox)
            {
                context.DrawRectangle(pen, SelectionRect);
            }

            foreach (Line connectionLine in GetConnectionLines())
            {
                StreamGeometry curve = new();
                using (StreamGeometryContext streamContext = curve.Open())
                {
                    int count = 0;

                    // Point lastPoint = default;
                    // Point lastLastPoint = default;
                    foreach (Point point in connectionLine.GetPoints())
                    {
                        if (count == 0)
                        {
                            streamContext.BeginFigure(point, false);
                        }
                        else
                        {
                            streamContext.LineTo(point);
                            /*
                            switch ((count - 1) % 3)
                            {
                                case 0:
                                    lastLastPoint = point;
                                    break;
                                case 1:
                                    lastPoint = point;
                                    break;
                                case 2:
                                    streamContext.CubicBezierTo(lastLastPoint, lastPoint, point);
                                    break;
                            }
                            */
                        }

                        count++;
                    }

                    streamContext.EndFigure(false);
                    streamContext.Dispose();
                }

                context.DrawGeometry(Brushes.Transparent, pen, curve);
                context.DrawGeometry(Brushes.Transparent, new Pen(connectionLine.FillBrush, FlatSize * 3.5), curve);
            }
        }

        public void SelectMouseDown(PointerPressedEventArgs e)
        {
            if (dragType == DragType.None)
            {
                originalClickPoint = e.GetPosition(this);
                if (e.KeyModifiers != KeyModifiers.Shift)
                {
                    DeselectAllNodes();
                }

                dragType = DragType.SelectionBox;
                e.Handled = true;
            }
        }

        public void SelectMouseUp(PointerReleasedEventArgs e)
        {
            switch (dragType)
            {
                case DragType.MovingNodes:
                    foreach (NodeDisplay node in selectedNodes)
                    {
                        node.CoreNode.X = node.Bounds.Left;
                        node.CoreNode.Y = node.Bounds.Top;
                    }

                    break;
                case DragType.CreatingCurve:
                    IVisual visualUnderPointer = VisualRoot.Renderer.HitTestFirst(e.GetCurrentPoint(VisualRoot).Position, VisualRoot, null);
                    if (visualUnderPointer is Control controlUnderPointer && controlUnderPointer.Tag is IConnector endConnector)
                    {
                        endConnector.Tag = controlUnderPointer;
                        manager.TryConnectFields(selectedField.Tag as IConnector, endConnector);
                    }

                    break;
            }

            InvalidateVisual();
            dragType = DragType.None;
        }

        public void SelectMouseMove(PointerEventArgs e)
        {
            switch (dragType)
            {
                case DragType.SelectionBox:
                    DeselectAllNodes();
                    currentMousePos = e.GetPosition(this);
                    foreach (IVisual visual in this.GetVisualChildren())
                    {
                        if (visual is NodeDisplay nodeDisplay && SelectionRect.Intersects(visual.Bounds))
                        {
                            SelectNode(nodeDisplay);
                        }
                    }

                    InvalidateVisual();
                    break;
                case DragType.MovingNodes:
                    Vector delta = e.GetPosition(this) - originalClickPoint;
                    foreach (NodeDisplay node in selectedNodes)
                    {
                        SetTop(node, node.CoreNode.Y + delta.Y);
                        SetLeft(node, node.CoreNode.X + delta.X);
                    }

                    InvalidateVisual();
                    break;
                case DragType.CreatingCurve:
                    currentMousePos = e.GetPosition(this);
                    if (VisualRoot.Renderer.HitTestFirst(e.GetPosition(VisualRoot), VisualRoot, null) is Control controlUnderPointer && controlUnderPointer.Tag is IConnector)
                    {
                        currentMousePos = GetCenterInLocal(controlUnderPointer);
                    }

                    InvalidateVisual();
                    break;
            }
        }

        internal void AddNode(NodeDisplay newNode, Point location = default)
        {
            newNode.CoreNode.X = location.X;
            newNode.CoreNode.Y = location.Y;
            manager.AddNode(newNode.CoreNode);
        }

        private void Nodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (object newItem in e.NewItems)
                    {
                        NodeAdded(newItem);
                    }

                    break;
            }
        }

        private void NodeAdded(object newItem)
        {
            INodeBase newNode = newItem as INodeBase;
            NodeDisplay newDisplay = new() { CoreNode = newNode };
            SetLeft(newDisplay, newNode.X);
            SetTop(newDisplay, newNode.Y);
            newDisplay.PointerPressed += Node_PointerPressed;
            Children.Add(newDisplay);
        }

        private void Node_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            PointerPoint click = e.GetCurrentPoint(this);
            if (click.Properties.IsLeftButtonPressed == true)
            {
                if (e.Source is Control sourceControl && sourceControl.Tag is IConnector interactedField)
                {
                    IConnector dragField = manager.ConnectionChanged(interactedField);
                    if (dragField.Tag == null)
                    {
                        dragField.Tag = e.Source;
                    }

                    selectedField = dragField.Tag as Control;
                    originalClickPoint = GetCenterInLocal(selectedField);
                    dragType = DragType.CreatingCurve;
                }
                else if (sender is NodeDisplay clickedNode)
                {
                    if (!clickedNode.Selected)
                    {
                        if (e.KeyModifiers != KeyModifiers.Shift)
                        {
                            DeselectAllNodes();
                        }

                        SelectNode(clickedNode);
                    }

                    originalClickPoint = e.GetPosition(this);
                    dragType = DragType.MovingNodes;
                }
            }
        }

        private void SelectNode(NodeDisplay node)
        {
            node.Selected = true;
            selectedNodes.Add(node);
        }

        private void DeselectAllNodes()
        {
            foreach (NodeDisplay node in selectedNodes)
            {
                node.Selected = false;
            }

            selectedNodes = new List<NodeDisplay>();
        }

        private Point GetCenterInLocal(IVisual visual)
            => visual.VisualParent.TranslatePoint(visual.Bounds.Center, this).Value + new Point(visual.RenderTransform.Value.M31, visual.RenderTransform.Value.M32);

        private bool GetConnectorLocation(IConnector connector, out Point location)
        {
            if (connector.Tag is Control control && control.Tag == connector)
            {
                location = GetCenterInLocal(control);
                return true;
            }
            else
            {
                foreach (IVisual visual in this.GetVisualChildren())
                {
                    if (visual is NodeDisplay nodeDisplay && nodeDisplay.CoreNode == connector.ParentNode)
                    {
                        foreach (IVisual nodeVisual in nodeDisplay.GetVisualDescendants())
                        {
                            if (nodeVisual is Control nodeControl && nodeControl.Tag == connector)
                            {
                                connector.Tag = nodeControl;
                                location = GetCenterInLocal(nodeControl);
                                return true;
                            }
                        }
                    }
                }
            }

            location = default;
            return false;
        }

        private IEnumerable<Line> GetConnectionLines()
        {
            if (dragType == DragType.CreatingCurve)
            {
                if (selectedField.Tag is IConnector connector && connector.ConnectionType == ConnectionType.Input)
                {
                    yield return new Line(currentMousePos, originalClickPoint, (selectedField as Shape).Fill);
                }
                else if (selectedField.Tag is IConnector connector1 && connector1.ConnectionType == ConnectionType.Output)
                {
                    yield return new Line(originalClickPoint, currentMousePos, (selectedField as Shape).Fill);
                }
            }

            foreach (NodeConnection nodeConnection in manager.GetConnections())
            {
                if (GetConnectorLocation(nodeConnection.Output, out Point startPoint) && GetConnectorLocation(nodeConnection.Input, out Point endPoint))
                {
                    yield return new Line(startPoint, endPoint, (nodeConnection.Output.Tag as Shape).Fill);
                }
            }
        }

        private struct Line
        {
            public Point StartPoint;
            public Point EndPoint;
            public IBrush FillBrush;

            public Line(Point startPoint, Point endPoint, IBrush fillBrush)
            {
                StartPoint = startPoint;
                EndPoint = endPoint;
                FillBrush = fillBrush;
            }

            public IEnumerable<Point> GetPoints()
            {
                double padding = Math.Min(Math.Abs(StartPoint.Y - EndPoint.Y) * 0.5, 75);
                Point avgPoint = (StartPoint + EndPoint) / 2;
                yield return StartPoint;
                yield return StartPoint.WithX(StartPoint.X + padding);
                if (EndPoint.X - StartPoint.X < padding * 2)
                {
                    yield return new Point(StartPoint.X + padding, avgPoint.Y);
                }
                else
                {
                    yield return avgPoint;
                }

                yield return avgPoint;
                if (EndPoint.X - StartPoint.X < padding * 2)
                {
                    yield return new Point(EndPoint.X - padding, avgPoint.Y);
                }
                else
                {
                    yield return avgPoint;
                }

                yield return EndPoint.WithX(EndPoint.X - padding);
                yield return EndPoint;
            }
        }
    }
}
