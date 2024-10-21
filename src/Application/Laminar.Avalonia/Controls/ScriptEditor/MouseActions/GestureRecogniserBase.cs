using Avalonia.Input;
using Avalonia.Input.GestureRecognizers;

namespace Laminar.Avalonia.Controls.ScriptEditor.MouseActions;

internal abstract class GestureRecogniserBase : IGestureRecognizer
{
    protected IPointer Pointer { get; set; }

    protected IInputElement Target { get; private set; }

    protected IGestureRecognizerActionsDispatcher Actions { get; private set; }

    public void Initialize(IInputElement target, IGestureRecognizerActionsDispatcher actions)
    {
        Target = target;
        Actions = actions;
    }

    public void PointerCaptureLost(IPointer pointer)
    {
        if (pointer == Pointer)
        {
            Pointer = null;
            EndGesture();
        }
    }

    public void PointerMoved(PointerEventArgs e)
    {
        if (e.Pointer == Pointer)
        {
            Actions.Capture(e.Pointer, this);
            TrackedPointerMoved(e);
        }
    }

    public abstract void PointerPressed(PointerPressedEventArgs e);

    public virtual void PointerReleased(PointerReleasedEventArgs e)
    {
        PointerCaptureLost(e.Pointer);
    }

    protected virtual void EndGesture() { }

    protected abstract void TrackedPointerMoved(PointerEventArgs e);

    protected void Track(IPointer pointer)
    {
        Pointer = pointer;
    }
}
