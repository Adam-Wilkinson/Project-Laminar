using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using Laminar_Core.NodeSystem.NodeComponents.Visuals;
using Laminar_Core.NodeSystem.Nodes;
using Laminar_Core.NodeSystem.NodeTreeSystem;
using Laminar_Core.Scripting.Advanced.Editing;
using Laminar_Core.Scripting.Advanced.Editing.Connection;

namespace Laminar_Avalonia.NodeDisplaySystem
{
    public class NodeDisplayCanvas : Canvas
    {
        private readonly Pen pen = new(Brushes.LightGray);
        private IAdvancedScriptEditor _editor;
        private List<NodeDisplay> selectedNodes = new();
        private Point originalClickPoint;
        private bool hasClickPoint = false;
        private DragType dragType = DragType.None;
        private Control selectedField;
        private Point currentMousePos;

        public NodeDisplayCanvas()
        {
            DataContextChanged += NodeDisplayCanvas_DataContextChanged;
        }

        private void NodeDisplayCanvas_DataContextChanged(object sender, EventArgs e)
        {
            if (DataContext is IAdvancedScriptEditor editor)
            {
                _editor = editor;
                foreach (INodeContainer node in _editor.Nodes)
                {
                    NodeAdded(node);
                }

                ((INotifyCollectionChanged)_editor.Nodes).CollectionChanged += Nodes_CollectionChanged;
            }
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

                    Point lastPoint = default;
                    Point lastLastPoint = default;
                    foreach (Point point in connectionLine.GetPoints())
                    {
                        if (count == 0)
                        {
                            streamContext.BeginFigure(point, false);
                        }
                        else
                        {
                            // streamContext.LineTo(point);
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
                hasClickPoint = true;
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
                        node.CoreNode.Location.X = node.Bounds.Left;
                        node.CoreNode.Location.Y = node.Bounds.Top;
                    }

                    break;
                case DragType.CreatingCurve:
                    IVisual visualUnderPointer = VisualRoot.Renderer.HitTestFirst(e.GetCurrentPoint(VisualRoot).Position, VisualRoot, null);
                    if (visualUnderPointer is Control controlUnderPointer && controlUnderPointer.Tag is IConnector endConnector)
                    {
                        endConnector.Tag = controlUnderPointer;
                        _editor.TryConnectFields(selectedField.Tag as IConnector, endConnector);
                    }

                    break;
            }

            InvalidateVisual();
            hasClickPoint = false;
            dragType = DragType.None;
        }

        public void SelectMouseMove(PointerEventArgs e)
        {
            if (hasClickPoint == false)
            {
                originalClickPoint = e.GetPosition(this);
                hasClickPoint = true;
            }

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
                        SetTop(node, node.CoreNode.Location.Y + delta.Y);
                        SetLeft(node, node.CoreNode.Location.X + delta.X);
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
            newNode.CoreNode.Location.X = location.X;
            newNode.CoreNode.Location.Y = location.Y;
            _editor.AddNode(newNode.CoreNode);
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
            INodeContainer newNode = newItem as INodeContainer;
            NodeDisplay newDisplay = new() { CoreNode = newNode };
            SetLeft(newDisplay, newNode.Location.X);
            SetTop(newDisplay, newNode.Location.Y);
            newDisplay.PointerPressed += Node_PointerPressed;
            Children.Add(newDisplay);
        }

        internal void DeleteSelectedNodes()
        {
            foreach (NodeDisplay display in selectedNodes)
            {
                _editor.DeleteNode(display.CoreNode);
                Children.Remove(display);
            }
        }

        private void Node_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            PointerPoint click = e.GetCurrentPoint(this);
            if (click.Properties.IsLeftButtonPressed == true)
            {
                if (e.Source is Control sourceControl && sourceControl.Tag is IConnector interactedField)
                {
                    IConnector dragField = _editor.GetActiveConnector(interactedField);
                    if (dragField.Tag == null)
                    {
                        dragField.Tag = e.Source;
                    }

                    selectedField = dragField.Tag as Control;
                    hasClickPoint = true;
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

                    hasClickPoint = true;
                    originalClickPoint = e.GetPosition(this);
                    dragType = DragType.MovingNodes;
                }
            }
        }

        internal void DuplicateSelectedNodes()
        {
            foreach (NodeDisplay node in selectedNodes)
            {
                INodeContainer newNode = node.CoreNode.DuplicateNode();
                newNode.IsLive = true;
                AddNode(new NodeDisplay() { CoreNode = newNode }, node.Bounds.TopLeft);
            }
            hasClickPoint = false;
            dragType = DragType.MovingNodes;
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
                    if (visual is NodeDisplay nodeDisplay && (nodeDisplay.CoreNode.Fields as IList).Cast<IVisualNodeComponentContainer>().Any(x => x.InputConnector == connector || x.OutputConnector == connector))
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
                if (selectedField.Tag is IConnector connector && connector.ConnectorType == ConnectorType.Input)
                {
                    yield return new Line(currentMousePos, originalClickPoint, (selectedField as Shape).Fill);
                }
                else if (selectedField.Tag is IConnector connector1 && connector1.ConnectorType == ConnectorType.Output)
                {
                    yield return new Line(originalClickPoint, currentMousePos, (selectedField as Shape).Fill);
                }
            }

            foreach (INodeConnection nodeConnection in _editor.Connections)
            {
                if (GetConnectorLocation(nodeConnection.OutputConnector, out Point startPoint) && GetConnectorLocation(nodeConnection.InputConnector, out Point endPoint))
                {
                    yield return new Line(startPoint, endPoint, (nodeConnection.OutputConnector.Tag as Shape).Fill);
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
