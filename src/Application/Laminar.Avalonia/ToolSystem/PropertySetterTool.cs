using System;
using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

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

        if (DefaultPopupTarget is null)
        {
            
        }
        
        return new Binding { Converter = new GetPropertySetterCommandConverter(PropertyBinding, CanChangeValueBinding, DefaultPopupTarget, DescriptionBinding) };
    }
}

public class GetPropertySetterCommandConverter(IBinding? propertyBinding, IBinding? canExecuteBinding, Control? popupTarget, IBinding? descriptionBinding) : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var boundPropertyContainer = new BoundPropertyContainer
        {
            DataContext = value,
        };

        if (propertyBinding is not null)
        {
            boundPropertyContainer[!BoundPropertyContainer.ValueProperty] = propertyBinding;
        }
        
        if (canExecuteBinding is not null)
        {
            boundPropertyContainer[!BoundPropertyContainer.IsUserEditableProperty] = canExecuteBinding;
        }

        if (descriptionBinding is not null)
        {
            boundPropertyContainer[!StyledElement.NameProperty] = descriptionBinding;
        }
        
        return new PropertySetterCommand(boundPropertyContainer, popupTarget);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class BoundPropertyContainer : StyledElement, IInterfaceData
{
    public static readonly StyledProperty<object> ValueProperty =
        AvaloniaProperty.Register<BoundPropertyContainer, object>(nameof(Value));

    public static readonly StyledProperty<bool> IsUserEditableProperty =
        AvaloniaProperty.Register<BoundPropertyContainer, bool>(nameof(IsUserEditable), defaultValue: true);
    
    public object Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    
    public IUserInterfaceDefinition? Definition { get; }

    public bool IsUserEditable
    {
        get => GetValue(IsUserEditableProperty);
        set => SetValue(IsUserEditableProperty, value);
    }

    string IInterfaceData.Name => Name ?? string.Empty;
}

public class PropertySetterCommand : ICommand
{
    private readonly BoundPropertyContainer _boundPropertyContainer;
    private readonly Control? _popupTarget;
    
    public PropertySetterCommand(BoundPropertyContainer boundPropertyContainer, Control? popupTarget)
    {
        _popupTarget = popupTarget;
        _boundPropertyContainer = boundPropertyContainer;
        _boundPropertyContainer.PropertyChanged += (_, args) =>
        {
            if (args.Property == BoundPropertyContainer.IsUserEditableProperty)
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        };
    }

    public bool CanExecute(object? parameter) => _boundPropertyContainer.IsUserEditable;

    public void Execute(object? parameter)
    {
        if (!CanExecute(parameter)) return;
        
        if (_boundPropertyContainer.Value is bool booleanValue)
        {
            _boundPropertyContainer.Value = !booleanValue;
            return;
        }

        if (_popupTarget is null)
        {
            return;
        }

        var flyoutContent = new ContentControl
        {
            MinWidth = 250,
            Content = _boundPropertyContainer.Value is IInterfaceData ? _boundPropertyContainer.Value : _boundPropertyContainer,
        };

        void KeyDown(object? sender, KeyEventArgs e) => flyoutContent.Presenter?.Child?.RaiseEvent(e);

        void KeyUp(object? sender, KeyEventArgs e) => flyoutContent.Presenter?.Child?.RaiseEvent(e);

        _popupTarget.KeyDown += KeyDown;
        _popupTarget.KeyUp += KeyUp;
        
        var flyout = new Flyout { Content = flyoutContent, Placement = PlacementMode.Pointer };
        
        FlyoutBase.SetAttachedFlyout(_popupTarget, flyout);
        
        FlyoutBase.ShowAttachedFlyout(_popupTarget);

        flyout.Closed += (_, _) =>
        {
            _popupTarget.KeyDown -= KeyDown;
            _popupTarget.KeyUp -= KeyUp;
        };
    }

    public event EventHandler? CanExecuteChanged;
}