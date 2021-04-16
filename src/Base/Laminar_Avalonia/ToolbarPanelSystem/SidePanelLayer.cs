namespace Laminar_Avalonia.ToolbarPanelSystem
{
    using System;
    using System.Collections.Generic;
    using Avalonia;
    using Avalonia.Animation;
    using Avalonia.Animation.Easings;
    using Avalonia.Controls;
    using Avalonia.Controls.Shapes;
    using Avalonia.Layout;

    internal class SidePanelLayer : Animatable
    {
        public static readonly StyledProperty<double> DepthProperty = AvaloniaProperty.Register<SidePanelLayer, double>(nameof(Depth), SidePanel.Padding);

        private readonly SidePanel parent;

        public SidePanelLayer(int layerIndex, SidePanel parent)
        {
            LayerIndex = layerIndex;
            Controls = new List<ToolbarManager>();
            this.parent = parent;
            LayerResizer.Cursor = this.parent.ChangePanelWidthCursor;

            Transitions = new Transitions()
            {
                new DoubleTransition() { Property = DepthProperty, Duration = ToolbarPanel.AnimationDuration, Easing = new ExponentialEaseOut() },
            };
            this.GetObservable(DepthProperty).Subscribe((_) =>
            {
                DrawingCanvas?.Parent?.InvalidateArrange();
            });
        }

        public static double DefaultWidth { get; set; }

        public int LayerIndex { get; }

        public List<ToolbarManager> Controls { get; set; }

        public Line LayerResizer { get; } = new Line() { Classes = new Classes("ResizingLine") };

        public Canvas DrawingCanvas { get; private set; }

        public double Depth
        {
            get => GetValue(DepthProperty);
            set => SetValue(DepthProperty, value);
        }

        public void AddControlAt(int index, ToolbarManager grip, double relativeGirth = 0.0)
        {
            if (DrawingCanvas == null)
            {
                DrawingCanvas = grip.GripCanvas;
                DrawingCanvas?.Children.Add(LayerResizer);
            }

            if (Controls.Count == 0)
            {
                Depth = DefaultWidth + SidePanel.Padding;
            }

            index = Math.Max(index, Controls.Count);
            grip.ToolbarWidth = relativeGirth == 0.0 ? 1.0 / (Controls.Count + 1) : relativeGirth;
            grip.Index = index;
            grip.SidePanelLayer = this;
            for (int i = 0; i < Controls.Count; i++)
            {
                Controls[i].ToolbarWidth -= grip.ToolbarWidth / Controls.Count;
                Controls[i].Index = i + (i > index ? 1 : 0);
            }

            Controls.Insert(index, grip);

            if (Controls.Count > 1)
            {
                ToolbarManager addResizingGrip = index == 0 || index < Controls.Count - 1 ? grip : Controls[index - 1];
                addResizingGrip.Resizer.Cursor = parent.ChangeControlWidthCursor;
                addResizingGrip.GripCanvas?.Children.Add(addResizingGrip.Resizer);
            }
        }

        public void RemoveControlAt(int index)
        {
            ToolbarManager grip = Controls[index];

            ToolbarManager gripWithResizer = index < Controls.Count - 1 || index == 0 ? grip : Controls[index - 1];
            DrawingCanvas?.Children.Remove(gripWithResizer.Resizer);

            grip.SidePanelLayer = null;
            grip.Index = -1;

            Controls.RemoveAt(index);

            for (int i = 0; i < Controls.Count; i++)
            {
                Controls[i].ToolbarWidth += grip.ToolbarWidth / Controls.Count;
                Controls[i].Index = i;
            }

            if (Controls.Count == 0)
            {
                Depth = 0;
            }
        }
    }
}
