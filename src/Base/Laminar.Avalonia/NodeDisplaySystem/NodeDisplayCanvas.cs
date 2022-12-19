using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using Laminar.Avalonia.Utils;
using Laminar.Contracts.NodeSystem;
using Laminar_Core.Scripting.Advanced.Editing;

namespace Laminar.Avalonia.NodeDisplaySystem;

public class NodeDisplayCanvas : Canvas
{
    internal readonly NodeControlManager _nodeControlFinder = new();
    private readonly Pen pen = new(Brushes.LightGray);
    private IAdvancedScriptEditor _editor;
    private Point originalClickPoint;
    private bool hasClickPoint = false;
    private DragType dragType = DragType.None;
    private Control selectedField;
    private Point currentMousePos;

    public NodeDisplayCanvas()
    {
        _editor = App.LaminarInstance.Factory.GetImplementation<IAdvancedScriptEditor>();
    }

    public double FlatSize { get; set; } = 1;

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        pen.Thickness = FlatSize * 5;
    }

    public void SelectMouseDown(PointerPressedEventArgs e)
    {
        if (dragType == DragType.None)
        {
            hasClickPoint = true;
            originalClickPoint = e.GetPosition(this);
            e.Handled = true;
        }
    }

    public void SelectMouseUp(PointerReleasedEventArgs e)
    {
        switch (dragType)
        {
            case DragType.CreatingCurve:
                IVisual visualUnderPointer = VisualRoot.Renderer.HitTestFirst(e.GetCurrentPoint(VisualRoot).Position, VisualRoot, null);
                if (visualUnderPointer is Control controlUnderPointer && controlUnderPointer.Tag is Laminar_Core.Scripting.Advanced.Editing.Connection.IConnector endConnector)
                {
                    endConnector.Tag = controlUnderPointer;
                    _editor.TryConnectFields(selectedField.Tag as Laminar_Core.Scripting.Advanced.Editing.Connection.IConnector, endConnector, out _);
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
            case DragType.CreatingCurve:
                currentMousePos = e.GetPosition(this);
                if (VisualRoot.Renderer.HitTestFirst(e.GetPosition(VisualRoot), VisualRoot, null) is Control controlUnderPointer && controlUnderPointer.Tag is Laminar_Core.Scripting.Advanced.Editing.Connection.IConnector)
                {
                    currentMousePos = GetCenterInLocal(controlUnderPointer);
                }

                InvalidateVisual();
                break;
        }
    }

    internal void AddNode(INodeWrapper newNode, Point location = default)
    {
        newNode.Location.Value = ValueObjectConverter.Point(location);
        _editor.AddNode(newNode);
        NodeAdded(newNode);
    }

    private void NodeAdded(object newItem)
    {
        INodeWrapper newNode = newItem as INodeWrapper;
        Control nodeControl = _nodeControlFinder.GetControl(newNode);
        SetLeft(nodeControl, newNode.Location.Value.X);
        SetTop(nodeControl, newNode.Location.Value.Y);
        nodeControl.PointerPressed += Node_PointerPressed;
        Children.Add(nodeControl);
    }

    private void Node_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        PointerPoint click = e.GetCurrentPoint(this);
        if (click.Properties.IsLeftButtonPressed == true)
        {
            if (e.Source is Control sourceControl && sourceControl.Tag is Laminar_Core.Scripting.Advanced.Editing.Connection.IConnector interactedField)
            {
                Laminar_Core.Scripting.Advanced.Editing.Connection.IConnector dragField = _editor.GetActiveConnector(interactedField);
                if (dragField.Tag == null)
                {
                    dragField.Tag = e.Source;
                }

                selectedField = dragField.Tag as Control;
                hasClickPoint = true;
                originalClickPoint = GetCenterInLocal(selectedField);
                dragType = DragType.CreatingCurve;
            }
        }
    }

    private Point GetCenterInLocal(IVisual visual)
        => visual.VisualParent.TranslatePoint(visual.Bounds.Center, this).Value + new Point(visual.RenderTransform.Value.M31, visual.RenderTransform.Value.M32);

    private enum DragType
    {
        CreatingCurve,
        None,
    }
}
