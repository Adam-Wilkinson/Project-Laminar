using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Reactive;
using Laminar.Avalonia.DragDrop;

namespace Laminar.Avalonia.Controls;

[TemplatePart(DragTargetName, typeof(Control), IsRequired = true)]
public class ItemPickerItem : ContentControl
{
    public const string DragTargetName = "PART_DragTarget";

    private Control? _dragTarget;
    private IDisposable? _dragTargetBeingDraggedObservable;
    
    public static readonly DirectProperty<ItemPickerItem, bool> IsHoveredOrDraggingProperty = AvaloniaProperty.RegisterDirect<ItemPickerItem, bool>(nameof(IsHoveredOrDragging), ipi => ipi.IsHoveredOrDragging);
    
    public bool IsHoveredOrDragging
    {
        get;
        set => SetAndRaise(IsHoveredOrDraggingProperty, ref field, value);
    }
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _dragTarget = e.NameScope.Find<Control>(DragTargetName)!;
        DragDrop.DragDrop.SetTriggerMouseButton(_dragTarget, MouseButton.Left);
        _dragTargetBeingDraggedObservable?.Dispose();
        _dragTargetBeingDraggedObservable = _dragTarget.GetObservable(BeingDraggedHandler.IsBeingDraggedProperty)
            .Subscribe(new AnonymousObserver<bool>(x =>
            {
                IsHoveredOrDragging = x;
            }));
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        IsHoveredOrDragging = true;
        base.OnPointerEntered(e);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        IsHoveredOrDragging = _dragTarget is not null && BeingDraggedHandler.GetIsBeingDragged(_dragTarget);
        base.OnPointerExited(e);
    }
}