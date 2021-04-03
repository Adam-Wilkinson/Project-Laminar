namespace OpenFlow_Avalonia.ToolbarPanelSystem
{
    using System;
    using System.Collections.Generic;
    using Avalonia;
    using Avalonia.Animation;
    using Avalonia.Animation.Easings;
    using Avalonia.Input;
    using Avalonia.Layout;

    internal class SidePanel
    {
        private readonly List<SidePanelLayer> layers = new();
        private readonly bool reverse;
        private readonly double toolbarDragOverlap = 20;

        private double totalAvailableWidth;
        private Rect bounds;
        private List<DragRecepticle> toolbarDragHitBoxes = new();

        public SidePanel(Orientation orientation, bool reverse = false)
        {
            Orientation = orientation;
            this.reverse = reverse;
        }

        public static double Padding { get; set; }

        public Cursor ChangePanelWidthCursor => Orientation == Orientation.Horizontal ? new Cursor(StandardCursorType.SizeNorthSouth) : new Cursor(StandardCursorType.SizeWestEast);

        public Cursor ChangeControlWidthCursor => Orientation == Orientation.Horizontal ? new Cursor(StandardCursorType.SizeWestEast) : new Cursor(StandardCursorType.SizeNorthSouth);

        public Orientation Orientation { get; }

        private double RelativePadding => Padding / bounds.Width;

        public static void EndClick(DragContext dragContext)
        {
            dragContext.SidePanelLayer.Transitions.Insert(0, new DoubleTransition()
            {
                Property = SidePanelLayer.DepthProperty,
                Duration = ToolbarPanel.AnimationDuration,
                Easing = new ExponentialEaseOut(),
            });
        }

        public void AddToolbar(ToolbarManager grip, double relativeGirth = 0.0)
        {
            int level = grip.Level;
            int i = 0;
            if (layers.Count == 0)
            {
                layers.Add(new SidePanelLayer(level, this));
            }
            else
            {
                while (i < layers.Count && layers[i].LayerIndex < level)
                {
                    i++;
                }

                if (i >= layers.Count || layers[i].LayerIndex != level)
                {
                    layers.Insert(i, new SidePanelLayer(level, this));
                }
            }

            layers[i].AddControlAt(layers[i].Controls.Count, grip, relativeGirth);
            grip.Orientation = Orientation;
        }

        public Rect Arrange(Rect currentRect1)
        {
            Rect currentRect = FixOrientation(currentRect1);
            bounds = currentRect.Inflate(new Thickness(Padding / 2, 0, Padding / 2, 0));
            totalAvailableWidth = currentRect.Height;
            double totalSize = 0.0;
            double accumulatedWidth;
            foreach (SidePanelLayer sidePanelLayer in layers)
            {
                accumulatedWidth = 0;
                foreach (ToolbarManager grip in sidePanelLayer.Controls)
                {
                    grip.ToolbarStart = accumulatedWidth;
                    grip.ToolbarEnd = accumulatedWidth + grip.ToolbarWidth + grip.WidthDelta;
                    grip.ControlSize = ToGlobal(new Size(grip.ToolbarEnd - grip.ToolbarStart - RelativePadding, sidePanelLayer.Depth));
                    if (grip.IsPlaced)
                    {
                        grip.UpdatePositions(ToGlobal(new Rect(
                            grip.ToolbarStart + (RelativePadding / 2),
                            totalSize,
                            grip.ToolbarEnd - grip.ToolbarStart - RelativePadding,
                            Math.Max(0.0, sidePanelLayer.Depth - Padding))));

                        if (grip.Resizer != null)
                        {
                            grip.Resizer.StartPoint = ToGlobal(new Point(grip.ToolbarEnd - (RelativePadding / 2), totalSize));
                            grip.Resizer.EndPoint = ToGlobal(new Point(grip.ToolbarEnd - (RelativePadding / 2), totalSize + sidePanelLayer.Depth - Padding));
                        }
                    }

                    accumulatedWidth += grip.ToolbarWidth + grip.WidthDelta;
                }

                totalSize += sidePanelLayer.Depth;

                if (true || sidePanelLayer.Controls.Count > 1 || (sidePanelLayer.Controls.Count > 0 && sidePanelLayer.Controls[0].IsPlaced))
                {
                    sidePanelLayer.LayerResizer.StartPoint = ToGlobal(new Point(RelativePadding, totalSize - (Padding / 2)));
                    sidePanelLayer.LayerResizer.EndPoint = ToGlobal(new Point(1 - RelativePadding, totalSize - (Padding / 2)));
                }
            }

            bounds = currentRect.WithHeight(totalSize);
            return FixOrientation(currentRect.WithY(reverse ? currentRect.Y : totalSize).WithHeight(currentRect.Height - totalSize));
        }

        public void PrepForToolbarDrag()
        {
            toolbarDragHitBoxes = new List<DragRecepticle>();
            double overlapFrac = 0.2;
            double accumulatedWidth = 0;
            double previousOverlap = SidePanelLayer.DefaultWidth * overlapFrac;

            double accumulatedGirth;
            double newControlSize;
            double newControlSizeDelta;

            int i = 0;
            while (i < layers.Count)
            {
                toolbarDragHitBoxes.Add(new DragRecepticle
                {
                    HitBox = new Rect(0, accumulatedWidth - previousOverlap, 1, previousOverlap + (layers[i].Depth * overlapFrac)),
                    SidePanelLayerIndex = i,
                });

                if (layers[i].Controls.Count > 0)
                {
                    layers.Insert(i, new SidePanelLayer(0, this));
                    i++;
                    previousOverlap = layers[i].Depth * overlapFrac;
                    newControlSize = 1.0 / (layers[i].Controls.Count + 1);
                    newControlSizeDelta = newControlSize / layers[i].Controls.Count;
                    toolbarDragHitBoxes.Add(new DragRecepticle
                    {
                        HitBox = new Rect(0, accumulatedWidth, newControlSize, layers[i].Depth),
                        SidePanelLayerIndex = i,
                    });
                    accumulatedGirth = layers[i].Controls[0].ToolbarWidth - newControlSizeDelta;
                    for (int j = 0; j < layers[i].Controls.Count; j++)
                    {
                        toolbarDragHitBoxes.Add(new DragRecepticle
                        {
                            HitBox = new Rect(accumulatedGirth, accumulatedWidth, newControlSize, layers[i].Depth),
                            SidePanelLayerIndex = i,
                            Index = j + 1,
                        });
                        accumulatedGirth += layers[i].Controls[j].ToolbarWidth - newControlSizeDelta;
                    }

                    accumulatedWidth += layers[i].Depth + Padding;
                }

                i++;
            }

            toolbarDragHitBoxes.Add(new DragRecepticle
            {
                HitBox = new Rect(0, accumulatedWidth - previousOverlap, 1, previousOverlap * 2),
                SidePanelLayerIndex = i,
            });
            layers.Insert(i, new SidePanelLayer(0, this));
        }

        public void FinishControlDrag()
        {
            int i = 0;
            while (i < layers.Count)
            {
                if (layers[i].Controls.Count == 0)
                {
                    layers[i].DrawingCanvas?.Children.Remove(layers[i].LayerResizer);
                    layers.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        public bool TryPlaceControl(ToolbarManager grip, Point p)
        {
            p = ToLocal(p);
            if (!IsInside(p, toolbarDragOverlap))
            {
                return false;
            }

            foreach (DragRecepticle recepticle in toolbarDragHitBoxes)
            {
                if (recepticle.HitBox.Contains(p))
                {
                    if (recepticle.SidePanelLayerIndex < layers.Count && grip.SidePanelLayer == layers[recepticle.SidePanelLayerIndex] && grip.Index == recepticle.Index)
                    {
                        return true;
                    }

                    grip.TryRemoveFromParent();
                    layers[recepticle.SidePanelLayerIndex].AddControlAt(recepticle.Index, grip);
                    grip.Orientation = Orientation;
                    return true;
                }
            }

            return false;
        }

        public DragContext SendClick(Point p)
        {
            p = ToLocal(p);
            if (!IsInside(p, Padding * 2))
            {
                return default;
            }

            foreach (SidePanelLayer sidePanelLayer in layers)
            {
                if (sidePanelLayer.LayerResizer.IsPointerOver)
                {
                    sidePanelLayer.Transitions.RemoveAt(0);
                    return new DragContext { SidePanelLayer = sidePanelLayer, InitialWidth = sidePanelLayer.Depth, InitialClickPoint = p, DragMode = DragMode.ResizingWidth };
                }

                foreach (ToolbarManager grip in sidePanelLayer.Controls)
                {
                    if (grip.Resizer != null && grip.Resizer.IsPointerOver)
                    {
                        grip.RemoveEndTransition();
                        return new DragContext { InitialClickPoint = p, ToolbarGrip = grip, InitialWidth = grip.ToolbarWidth, DragMode = DragMode.ResizingControl };
                    }
                }
            }

            return default;
        }

        public void MovedClick(Point p, DragContext dragContext)
        {
            p = ToLocal(p);
            if (dragContext.DragMode == DragMode.ResizingWidth)
            {
                dragContext.SidePanelLayer.Depth = dragContext.InitialWidth + p.Y - dragContext.InitialClickPoint.Y;
            }
            else if (dragContext.DragMode == DragMode.ResizingControl)
            {
                dragContext.ToolbarGrip.WidthDelta = p.X - dragContext.InitialClickPoint.X;
                dragContext.ToolbarGrip.SidePanelLayer.Controls[dragContext.ToolbarGrip.Index + 1].WidthDelta = -p.X + dragContext.InitialClickPoint.X;
            }
        }

        private Rect FixOrientation(Rect globalRect)
        {
            if (Orientation == Orientation.Vertical)
            {
                return new Rect(globalRect.Y, globalRect.X, globalRect.Height, globalRect.Width);
            }

            return globalRect;
        }

        private Point ToLocal(Point p)
        {
            double x = p.X;
            double y = p.Y;
            if (Orientation == Orientation.Vertical)
            {
                x = p.Y;
                y = p.X;
            }

            y -= bounds.Y;
            y = reverse ? totalAvailableWidth - y : y;
            x = (x - bounds.X) / bounds.Width;
            return new Point(x, y);
        }

        private Point ToGlobal(Point p)
        {
            double x = p.X;
            double y = p.Y;
            y = (reverse ? totalAvailableWidth - y : y) + bounds.Y;
            x = (x * bounds.Width) + bounds.X;
            if (Orientation == Orientation.Vertical)
            {
                double temp = x;
                x = y;
                y = temp;
            }

            return new Point(x, y);
        }

        private Rect ToGlobal(Rect r)
        {
            double x = r.X;
            double y = r.Y;
            double width = r.Width;
            double height = r.Height;
            y = (reverse ? totalAvailableWidth - y - height : y) + bounds.Y;
            x = (x * bounds.Width) + bounds.X;
            width *= bounds.Width;
            return FixOrientation(new Rect(x, y, width, height));
        }

        private Size ToGlobal(Size s)
        {
            double width = s.Width;
            double height = s.Height;
            width *= bounds.Width;
            if (Orientation == Orientation.Vertical)
            {
                double temp = width;
                width = height;
                height = temp;
            }

            return new Size(width, height);
        }

        private bool IsInside(Point p, double inflation = 0.0) => (p.X > 0) && (p.X < 1) && (p.Y < bounds.Height + inflation);

        private struct DragRecepticle
        {
            public Rect HitBox;
            public int SidePanelLayerIndex;
            public int Index;
        }
    }
}
