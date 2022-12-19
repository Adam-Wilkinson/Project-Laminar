using System;
using Avalonia;
using Avalonia.Controls;

namespace Laminar.Avalonia.Controls;

public class ChildDataContextController : Decorator
{
    public static readonly StyledProperty<object> ChildDataContextProperty = AvaloniaProperty.Register<ChildDataContextController, object>(nameof(ChildDataContext));

    public ChildDataContextController()
    {
        this.GetObservable(BoundsProperty).Subscribe(value =>
        {
            MinHeight = Math.Max(DesiredSize.Height, MinHeight);
            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center;
        });

        this.GetObservable(ChildProperty).Subscribe(value =>
        {
            if (value is not null)
            {
                if (value is Control control)
                {
                    control.VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center;
                }

                value.DataContext = ChildDataContext;
            }
        });

        this.GetObservable(ChildDataContextProperty).Subscribe(value =>
        {
            if (value is not null && Child is not null)
            {
                Child.DataContext = value;
            }
        });
    }

    public object ChildDataContext
    {
        get => GetValue(ChildDataContextProperty);
        set => SetValue(ChildDataContextProperty, value);
    }
}
