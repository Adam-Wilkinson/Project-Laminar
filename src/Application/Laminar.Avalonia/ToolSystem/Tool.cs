using System;
using System.Collections;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Metadata;

namespace Laminar.Avalonia.ToolSystem;

public class Toolbox : Tool
{
    [Content] public AvaloniaList<Tool> ChildrenContent => ChildTools;
}

public class CommandTool : Tool
{
    public static readonly StyledProperty<IBinding?> CommandBindingProperty =
        AvaloniaProperty.Register<CommandTool, IBinding?>(nameof(CommandBinding));

    [AssignBinding]
    public IBinding? CommandBinding
    {
        get => GetValue(CommandBindingProperty);
        set => SetValue(CommandBindingProperty, value);
    }

    protected override IBinding? GetCommandBinding() => CommandBinding;
}

public class Tool : AvaloniaObject, ITemplate<object?, ToolInstance?>, IEnumerable<Tool>
{
    public const string ToolRootKey = "ToolRoot"; 
    
    public static readonly StyledProperty<KeyGesture?> GestureProperty = AvaloniaProperty.Register<Tool, KeyGesture?>(nameof(Gesture), new KeyGesture(Key.None));

    public static readonly StyledProperty<IDataTemplate?> IconTemplateProperty = AvaloniaProperty.Register<Tool, IDataTemplate?>(nameof(IconTemplate));
    
    public static readonly StyledProperty<IBinding?> DescriptionBindingProperty = AvaloniaProperty.Register<Tool, IBinding?>(nameof(DescriptionBinding));

    public static readonly StyledProperty<object?> CommandParameterProperty = AvaloniaProperty.Register<Tool, object?>(nameof(CommandParameter));
    
    public string Name { get; set; } = string.Empty;

    public KeyGesture? Gesture
    {
        get => GetValue(GestureProperty);
        set => SetValue(GestureProperty, value);
    }
    
    public IDataTemplate? IconTemplate
    {
        get => GetValue(IconTemplateProperty);
        set => SetValue(IconTemplateProperty, value);
    }
    
    [AssignBinding]
    public IBinding? DescriptionBinding
    {
        get => GetValue(DescriptionBindingProperty);
        set => SetValue(DescriptionBindingProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
    
    public Geometry? DefaultIconGeometry
    {
        get;
        set
        {
            field = value;
            if (value is not null && IconTemplate is null)
            {
                IconTemplate = new FuncDataTemplate(_ => true, (_, _) => new GeometryIcon { Data = value });   
            }
        }
    }

    public Control? DefaultPopupTarget { get; set; }

    public AvaloniaList<Tool> ChildTools { get; } = [];
    
    public ToolInstance? Build(object? param)
    {
        if (DataType is not null && !DataType.IsInstanceOfType(param))
        {
            return null;
        }
        
        var toolInstance = new ToolInstance
        {
            DataContext = param,
            Tool = this,
            PopupTarget = DefaultPopupTarget,
        };

        if (GetCommandBinding() is { } commandBinding)
            toolInstance[!ToolInstance.CommandProperty] = commandBinding;

        if (DescriptionBinding is { } descriptionBinding)
            toolInstance[!ToolInstance.DescriptionProperty] = descriptionBinding;

        return toolInstance;
    }
    
    protected virtual IBinding? GetCommandBinding() => null;

    public IEnumerator<Tool> GetEnumerator() => ChildTools.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    [DataType] 
    public Type? DataType { get; set; }
}
