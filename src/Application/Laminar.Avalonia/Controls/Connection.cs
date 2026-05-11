using System;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Laminar.Avalonia.Controls;

public class Connection : Shape
{
    private Layoutable? _startVisualCommonAncestor;
    private Layoutable? _endVisualCommonAncestor;
    
    public static readonly StyledProperty<Visual?> StartVisualProperty = AvaloniaProperty.Register<Connection, Visual?>(nameof(StartVisual));

    public static readonly StyledProperty<Visual?> EndVisualProperty = AvaloniaProperty.Register<Connection, Visual?>(nameof(EndVisual));

    static Connection()
    {
        StartVisualProperty.Changed.AddClassHandler<Connection>((con, args) => con.OnStartVisualChanged(args));
        EndVisualProperty.Changed.AddClassHandler<Connection>((con, args) => con.OnEndVisualChanged(args));
    }

    private void OnStartVisualChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (_startVisualCommonAncestor is not null && !Equals(_startVisualCommonAncestor, _endVisualCommonAncestor))
        {
            _startVisualCommonAncestor.LayoutUpdated -= CommonAncestorLayoutUpdated;
        }

        _startVisualCommonAncestor = FindLayoutableCommonAncestor(args.GetNewValue<Visual?>());
        _startVisualCommonAncestor?.LayoutUpdated += CommonAncestorLayoutUpdated;
    }

    private void OnEndVisualChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (_endVisualCommonAncestor is not null && !Equals(_endVisualCommonAncestor, _startVisualCommonAncestor))
        {
            _endVisualCommonAncestor.LayoutUpdated -= CommonAncestorLayoutUpdated;
        }
        
        _endVisualCommonAncestor = FindLayoutableCommonAncestor(args.GetNewValue<Visual?>());
        _endVisualCommonAncestor?.LayoutUpdated += CommonAncestorLayoutUpdated;
    }
    
    private void CommonAncestorLayoutUpdated(object? sender, EventArgs e)
    {
        InvalidateGeometry();
    }

    public Visual? StartVisual
    {
        get => GetValue(StartVisualProperty);
        set => SetValue(StartVisualProperty, value);
    }

    public Visual? EndVisual
    {
        get => GetValue(EndVisualProperty);
        set => SetValue(EndVisualProperty, value);
    }
    
    protected override Geometry? CreateDefiningGeometry()
    {
        if (StartVisual?.TransformToVisual(this) is not { } startTransform ||
            EndVisual?.TransformToVisual(this) is not { } endTransform)
            return null;
            
        return new LineGeometry(StartVisual.Bounds.TransformToAABB(startTransform).Center, EndVisual.Bounds.TransformToAABB(endTransform).Center);
    }

    private Layoutable? FindLayoutableCommonAncestor(Visual? visual)
    {
        if (visual?.FindCommonVisualAncestor(this) is not { } commonVisualAncestor) return null;
        
        while (true)
        {
            switch (commonVisualAncestor)
            {
                case Layoutable layoutable:
                    return layoutable;
                case null:
                    return null;
                default:
                    commonVisualAncestor = commonVisualAncestor.GetVisualParent();
                    break;
            }
        }
    }
}