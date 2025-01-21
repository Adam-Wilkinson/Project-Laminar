using System;
using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Laminar.Avalonia.ToolSystem;

public class PropertySetterTool : Tool
{
    public static readonly StyledProperty<IBinding?> CanChangeValueBindingProperty = AvaloniaProperty.Register<PropertySetterTool, IBinding?>(nameof(CanChangeValueBinding));

    public static readonly StyledProperty<IBinding?> PropertyBindingProperty = AvaloniaProperty.Register<PropertySetterTool, IBinding?>(nameof(PropertyBinding));
    
    [AssignBinding]
    public IBinding? CanChangeValueBinding
    {
        get => GetValue(CanChangeValueBindingProperty);
        set => SetValue(CanChangeValueBindingProperty, value);
    }

    [AssignBinding]
    public IBinding? PropertyBinding
    {
        get => GetValue(PropertyBindingProperty);
        set => SetValue(PropertyBindingProperty, value);
    }
    
    protected override IBinding GetCommandBinding()
    {
        if (PropertyBinding is BindingBase bindingBase && bindingBase.Mode != BindingMode.TwoWay)
        {
            bindingBase.Mode = BindingMode.TwoWay;
        }
        
        return new Binding { Converter = new GetPropertySetterCommandConverter(PropertyBinding, CanChangeValueBinding) };
    }
}

public class GetPropertySetterCommandConverter(IBinding? propertyBinding, IBinding? canExecuteBinding) : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var boundPropertyContainer = new BoundPropertyContainer
        {
            DataContext = value,
        };

        if (propertyBinding is not null)
        {
            boundPropertyContainer[!BoundPropertyContainer.BoundValueProperty] = propertyBinding;
        }
        
        if (canExecuteBinding is not null)
        {
            boundPropertyContainer[!BoundPropertyContainer.CanChangeValueProperty] = canExecuteBinding;
        }
        
        return new PropertySetterCommand(boundPropertyContainer);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class BoundPropertyContainer : StyledElement
{
    public static readonly StyledProperty<object?> BoundValueProperty =
        AvaloniaProperty.Register<BoundPropertyContainer, object?>(nameof(BoundValue));

    public static readonly StyledProperty<bool> CanChangeValueProperty =
        AvaloniaProperty.Register<BoundPropertyContainer, bool>(nameof(CanChangeValue), defaultValue: true);
    
    public object? BoundValue
    {
        get => GetValue(BoundValueProperty);
        set => SetValue(BoundValueProperty, value);
    }

    public bool CanChangeValue
    {
        get => GetValue(CanChangeValueProperty);
        set => SetValue(CanChangeValueProperty, value);
    }
}

public class PropertySetterCommand : ICommand
{
    private readonly BoundPropertyContainer _boundPropertyContainer;

    public PropertySetterCommand(BoundPropertyContainer boundPropertyContainer)
    {
        _boundPropertyContainer = boundPropertyContainer;
        _boundPropertyContainer.PropertyChanged += (_, args) =>
        {
            if (args.Property == BoundPropertyContainer.CanChangeValueProperty)
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        };
    }

    public bool CanExecute(object? parameter) 
        => _boundPropertyContainer.BoundValue is not null && _boundPropertyContainer.CanChangeValue;

    public void Execute(object? parameter)
    {
        if (!CanExecute(parameter)) return;
        
        if (_boundPropertyContainer.BoundValue is bool booleanValue)
        {
            _boundPropertyContainer.BoundValue = !booleanValue;
            return;
        }

        var test = TopLevel.GetTopLevel(null)!;
        
        FlyoutBase.SetAttachedFlyout(TopLevel.GetTopLevel(null)!, new Flyout { Content = new TextBlock { Text = "Hello?" }});
        
        FlyoutBase.ShowAttachedFlyout(TopLevel.GetTopLevel(null)!);
    }

    public event EventHandler? CanExecuteChanged;
}