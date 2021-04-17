namespace Laminar_Avalonia.NodeDisplaySystem
{
    using System;
    using System.Diagnostics;
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Data;
    using Avalonia.Input;
    using Avalonia.Media;
    using Laminar_Core.NodeSystem;
    using Laminar_Core.NodeSystem.Nodes;
    using static System.Math;
    using static Laminar_Avalonia.Utils.MatrixHelper;

    public class ZoomBorder : Border, DragDropHandler.IDragRecepticle
    {
        public static readonly StyledProperty<MouseButton> PanButtonProperty = AvaloniaProperty.Register<ZoomBorder, MouseButton>(nameof(PanButton), MouseButton.Right, false, BindingMode.TwoWay);
        public static readonly StyledProperty<MouseButton> SelectButtonProperty = AvaloniaProperty.Register<ZoomBorder, MouseButton>(nameof(SelectButton), MouseButton.Left, false, BindingMode.TwoWay);
        public static readonly StyledProperty<double> ZoomSpeedProperty = AvaloniaProperty.Register<ZoomBorder, double>(nameof(ZoomSpeed), 1.2, false, BindingMode.TwoWay);
        public static readonly DirectProperty<ZoomBorder, double> ZoomProperty = AvaloniaProperty.RegisterDirect<ZoomBorder, double>(nameof(Zoom), o => o.Zoom, null, 1.0);
        public static readonly DirectProperty<ZoomBorder, double> OffsetXProperty = AvaloniaProperty.RegisterDirect<ZoomBorder, double>(nameof(OffsetX), o => o.OffsetX, null, 0.0);
        public static readonly DirectProperty<ZoomBorder, double> OffsetYProperty = AvaloniaProperty.RegisterDirect<ZoomBorder, double>(nameof(OffsetY), o => o.OffsetY, null, 0.0);

        private readonly double minZoom = 0.4;
        private readonly double maxZoom = 10;

        private readonly NodeDisplayCanvas nodeManager = new();
        private Point pan;
        private Point previous;
        private Matrix matrix = Matrix.Identity;
        private bool isPanning;
        private bool isSelectDragging;

        public ZoomBorder()
            : base()
        {
            Focusable = true;

            ClipToBounds = true;
            Child = nodeManager;
            Background = new SolidColorBrush(new Color(255, 11, 11, 11));
        }

        public MouseButton PanButton
        {
            get => GetValue(PanButtonProperty);
            set => SetValue(PanButtonProperty, value);
        }

        public MouseButton SelectButton
        {
            get => GetValue(SelectButtonProperty);
            set => SetValue(SelectButtonProperty, value);
        }

        public double ZoomSpeed
        {
            get => GetValue(ZoomSpeedProperty);
            set => SetValue(ZoomSpeedProperty, value);
        }

        public double Zoom { get; private set; } = 1.0;

        public double OffsetX { get; private set; }

        public double OffsetY { get; private set; }

        public void ZoomTo(double zoom, double x, double y)
        {
            matrix = ScaleAtPrepend(matrix, zoom, zoom, x, y);
            nodeManager.FlatSize = 1 / matrix.M11;
            Invalidate();
        }

        public void ZoomDeltaTo(double delta, double x, double y)
        {
            if ((delta > 0 && Zoom > maxZoom) || (delta < 0 && Zoom < minZoom))
            {
                return;
            }

            ZoomTo(delta > 0 ? ZoomSpeed : 1 / ZoomSpeed, x, y);
        }

        public void PanTo(double x, double y)
        {
            Point delta = new(x - previous.X, y - previous.Y);
            previous = new Point(x, y);
            pan = new Point(pan.X + delta.X, pan.Y + delta.Y);
            matrix = TranslatePrepend(matrix, pan.X, pan.Y);
            Invalidate();
        }

        public void AutoFit(double panelWidth, double panelHeight, double elementWidth, double elementHeight)
        {
            double zoom = Min(panelWidth / elementWidth, panelHeight / elementHeight);
            matrix = ScaleAt(zoom, zoom, elementWidth / 2.0, elementHeight / 2.0);
            Invalidate();
        }

        public void AutoFit()
        {
            AutoFit(Bounds.Width, Bounds.Height, nodeManager.Bounds.Width, nodeManager.Bounds.Height);
        }

        public bool EndDrag(DragDropHandler dragDropInstance, PointerReleasedEventArgs e)
        {
            if (dragDropInstance == null || e == null)
            {
                throw new ArgumentNullException(dragDropInstance == null ? nameof(dragDropInstance) : nameof(e));
            }

            if (dragDropInstance.DragTypeIdentifier == "NodeDisplay" && dragDropInstance.DragControl is NodeDisplay nodeToCopy)
            {
                INodeBase newNode = nodeToCopy.CoreNode.DuplicateNode();
                newNode.MakeLive();
                nodeManager.AddNode(new NodeDisplay() { CoreNode = newNode }, e.GetPosition(nodeManager) - dragDropInstance.ClickOffset);
                return true;
            }

            return false;
        }

        public void Invalidate()
        {
            double oldZoom = Zoom;
            double oldOffsetX = OffsetX;
            double oldOffsetY = OffsetY;
            Zoom = matrix.M11;
            OffsetX = matrix.M31;
            OffsetY = matrix.M32;
            RaisePropertyChanged(ZoomProperty, oldZoom, Zoom);
            RaisePropertyChanged(OffsetXProperty, oldOffsetX, OffsetX);
            RaisePropertyChanged(OffsetYProperty, oldOffsetY, OffsetY);
            nodeManager.RenderTransformOrigin = new RelativePoint(new Point(0, 0), RelativeUnit.Relative);
            nodeManager.RenderTransform = new MatrixTransform(matrix);
            nodeManager.InvalidateVisual();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var size = base.ArrangeOverride(finalSize);

            if (nodeManager.IsMeasureValid)
            {
                AutoFit(size.Width, size.Height, nodeManager.Bounds.Width, nodeManager.Bounds.Height);
            }

            return size;
        }

        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            if (!isPanning && e != null)
            {
                ZoomDeltaTo(e.Delta.Y, e.GetPosition(nodeManager).X, e.GetPosition(nodeManager).Y);
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (isPanning == false && isSelectDragging == false && e != null)
            {
                PointerPointProperties properties = e.GetCurrentPoint(this).Properties;
                if (IsButtonPressed(properties, PanButton))
                {
                    Point point = e.GetPosition(nodeManager);

                    pan = default;
                    previous = new Point(point.X, point.Y);

                    isPanning = true;
                }
                else if (IsButtonPressed(properties, SelectButton) && (nodeManager is NodeDisplayCanvas))
                {
                    nodeManager.SelectMouseDown(e);
                    isSelectDragging = true;
                }
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            if (isPanning)
            {
                isPanning = false;
            }
            else if (isSelectDragging)
            {
                nodeManager.SelectMouseUp(e);
                isSelectDragging = false;
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            if (isPanning)
            {
                Point point = e.GetPosition(nodeManager);
                PanTo(point.X, point.Y);
            }
            else if (isSelectDragging)
            {
                nodeManager.SelectMouseMove(e);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key is Key.D && e.KeyModifiers is KeyModifiers.Shift)
            {
                nodeManager.DuplicateSelectedNodes();
                isSelectDragging = true;
            }
        }

        private static bool IsButtonPressed(PointerPointProperties properties, MouseButton button)
        {
            return (properties.IsLeftButtonPressed && button == MouseButton.Left) || (properties.IsMiddleButtonPressed && button == MouseButton.Middle) || (properties.IsRightButtonPressed && button == MouseButton.Right);
        }
    }
}