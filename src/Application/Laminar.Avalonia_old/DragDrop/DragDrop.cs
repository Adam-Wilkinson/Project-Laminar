using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.VisualTree;
using ReactiveUI;

namespace Laminar.Avalonia.DragDrop;

public class DragDrop : AvaloniaObject
{
    public static readonly AttachedProperty<object> DragSourceControlProperty = AvaloniaProperty.RegisterAttached<DragDrop, Control, object>("DragSourceControl");

    public static object GetDragSourceControl(AvaloniaObject control) => control.GetValue(DragSourceControlProperty);

    public static void SetDragSourceControl(AvaloniaObject control, object obj) => control.SetValue(DragSourceControlProperty, obj);

    public static readonly AttachedProperty<ICommand> DragEndCommandProperty = AvaloniaProperty.RegisterAttached<DragDrop, Interactive, ICommand>("DragEndCommand");

    public static ICommand GetDragEndCommand(AvaloniaObject control) => control.GetValue(DragEndCommandProperty);

    public static void SetDragEndCommand(AvaloniaObject control, ICommand command) => control.SetValue(DragEndCommandProperty, command);


    private readonly static ContentControl _currentDragControl = new();
    private readonly static Canvas _adornerDrawingCanvas = new();

    private static object? _currentDragObject;
    private static Point? _clickOffset;

    static DragDrop()
    {
        DragSourceControlProperty.Changed.Subscribe(DragSourcePropertyChanged);

        _adornerDrawingCanvas.Children.Add(_currentDragControl);
    }

    private static void DragSourcePropertyChanged(AvaloniaPropertyChangedEventArgs<object> e)
    {
        if (e.Sender is not IInteractive inputElementSender)
        {
            throw new Exception($"Property {nameof(DragSourceControlProperty)} is only valid on objects of type {typeof(IInteractive)}");
        }

        inputElementSender.AddHandler(InputElement.PointerPressedEvent, InputElementSender_PointerPressed);
        inputElementSender.AddHandler(InputElement.PointerReleasedEvent, InputElementSender_PointerReleased);
        inputElementSender.AddHandler(InputElement.PointerMovedEvent, InputElementSender_PointerMoved);
    }

    private static void InputElementSender_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_currentDragObject is null || sender is not IVisual senderVisual || !_clickOffset.HasValue)
        {
            return;
        }

        Point newLocation = e.GetPosition(senderVisual.GetVisualRoot());
        Canvas.SetLeft(_currentDragControl, newLocation.X - _clickOffset.Value.X);
        Canvas.SetTop(_currentDragControl, newLocation.Y - _clickOffset.Value.Y);
    }

    private static void InputElementSender_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control senderControl || !e.GetCurrentPoint(senderControl).Properties.IsLeftButtonPressed || senderControl.GetValue(DragSourceControlProperty) is not object dragControl)
        {
            return;
        }

        if (AdornerLayer.GetAdornerLayer(senderControl) is not AdornerLayer previewLayer)
        {
            return;
        }

        previewLayer.DataTemplates.Add(new DataTemplateRedirect(senderControl as IControl));
        previewLayer.Children.Add(_adornerDrawingCanvas);

        _clickOffset = e.GetPosition(senderControl);
        Point startingPosition = e.GetPosition(senderControl.GetVisualRoot());
        _currentDragObject = dragControl;
        _currentDragControl.Content = _currentDragObject;
        Canvas.SetLeft(_currentDragControl, startingPosition.X - _clickOffset.Value.X);
        Canvas.SetTop(_currentDragControl, startingPosition.Y - _clickOffset.Value.Y);

        e.Handled = true;
    }

    private static void InputElementSender_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_currentDragObject is null || sender is not IVisual senderVisual || senderVisual.VisualRoot is not IRenderRoot root)
        {
            return;
        }

        foreach (IVisual visual in root.Renderer.HitTest(e.GetPosition(root), root, x => true))
        {
            if (visual is Control av && av.GetValue(DragEndCommandProperty) is ICommand endDragCommand)
            {
                endDragCommand.Execute(_currentDragObject);
            }
        }

        AdornerLayer.GetAdornerLayer(senderVisual)?.Children.Remove(_adornerDrawingCanvas);
        _currentDragObject = null;
    }
}