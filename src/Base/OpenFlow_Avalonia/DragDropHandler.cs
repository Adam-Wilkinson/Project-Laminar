namespace OpenFlow_Avalonia
{
    using System.Linq;
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Controls.Primitives;
    using Avalonia.Input;

    public class DragDropHandler
    {
        private readonly Canvas drawingCanvas = new();

        private DragDropHandler(PointerPressedEventArgs e, string dragTypeIdentifier, Control dragControl = null, object dragContext = null, Vector clickOffset = default)
        {
            if (!(e.Source is IControl))
            {
                return;
            }

            Source = e.Source as IControl;
            Source.PointerMoved += DrawingLayer_PointerMoved;
            Source.PointerReleased += Cleanup;

            if (dragControl != null)
            {
                drawingCanvas.Children.Add(dragControl);
                AdornerLayer.GetAdornerLayer(Source).Children.Add(drawingCanvas);

                Canvas.SetTop(dragControl, e.GetPosition(drawingCanvas).Y - clickOffset.Y);
                Canvas.SetLeft(dragControl, e.GetPosition(drawingCanvas).X - clickOffset.X);
            }

            this.DragControl = dragControl;
            this.DragContext = dragContext;
            this.DragTypeIdentifier = dragTypeIdentifier;
            this.ClickOffset = clickOffset;
        }

        public interface IDragRecepticle
        {
            public bool EndDrag(DragDropHandler dragDropInstance, PointerReleasedEventArgs e);
        }

        public IControl Source { get; }

        public Control DragControl { get; }

        public object DragContext { get; }

        public string DragTypeIdentifier { get; }

        public Vector ClickOffset { get; }

        public static void StartDrop(PointerPressedEventArgs e, string dragTypeIdentifier, Control dragControl = null, object dragContext = null, Vector clickOffset = default)
        {
            if (e != null)
            {
                _ = new DragDropHandler(e, dragTypeIdentifier, dragControl, dragContext, clickOffset);
            }
        }

        private void Cleanup(object sender, PointerReleasedEventArgs e)
        {
            if (DragControl != null)
            {
                AdornerLayer.GetAdornerLayer(Source).Children.Remove(drawingCanvas);
            }

            (Source.VisualRoot.Renderer.HitTest(e.GetPosition(Source.VisualRoot), Source.VisualRoot, null).Where(x => x is IDragRecepticle).FirstOrDefault() as IDragRecepticle)?.EndDrag(this, e);
            Source.PointerMoved -= DrawingLayer_PointerMoved;
            Source.PointerReleased -= Cleanup;
        }

        private void DrawingLayer_PointerMoved(object sender, PointerEventArgs e)
        {
            Canvas.SetTop(DragControl, e.GetPosition(drawingCanvas).Y - ClickOffset.Y);
            Canvas.SetLeft(DragControl, e.GetPosition(drawingCanvas).X - ClickOffset.X);
        }
    }
}
