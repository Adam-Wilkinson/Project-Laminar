using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Metadata;
using CommunityToolkit.Mvvm.Input;

namespace Laminar.Avalonia.ToolSystem;

public class Toolbox : Tool
{
    public Toolbox()
    {
        ChildTools = ChildrenContent;
        ChildrenContent.CollectionChanged += ChildrenChanged;
    }

    [Content] public AvaloniaList<Tool> ChildrenContent { get; } = [];
    
    private void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                LogicalChildren.InsertRange(e.NewStartingIndex, e.NewItems!.OfType<ILogical>().ToList());
                break;
            case NotifyCollectionChangedAction.Move:
                LogicalChildren.MoveRange(e.OldStartingIndex, e.OldItems!.Count, e.NewStartingIndex);
                break;
            case NotifyCollectionChangedAction.Remove:
                LogicalChildren.RemoveAll(e.OldItems!.OfType<ILogical>().ToList());
                break;
            case NotifyCollectionChangedAction.Replace:
                for (var i = 0; i < e.OldItems!.Count; ++i)
                {
                    var index = i + e.OldStartingIndex;
                    var child = (ILogical)e.NewItems![i]!;
                    LogicalChildren[index] = child;
                }
                break;
            case NotifyCollectionChangedAction.Reset:
                LogicalChildren.Clear();
                break;
            default:
                throw new NotSupportedException();
        }
    }
}

public class CommandTool : Tool
{
    public static readonly StyledProperty<BindingBase?> CommandBindingProperty =
        AvaloniaProperty.Register<CommandTool, BindingBase?>(nameof(CommandBinding));

    [AssignBinding]
    public BindingBase? CommandBinding
    {
        get => GetValue(CommandBindingProperty);
        set => SetValue(CommandBindingProperty, value);
    }

    protected override BindingBase? GetCommandBinding() => CommandBinding;
}

public partial class Tool : StyledElement, ITemplate<object?, ToolInstance?>, IEnumerable<Tool>
{
    public const string ToolRootKey = "ToolRoot"; 
    
    public static readonly StyledProperty<KeyGesture> GestureProperty = AvaloniaProperty.Register<Tool, KeyGesture>(nameof(Gesture), new KeyGesture(Key.None));

    public static readonly StyledProperty<IDataTemplate?> IconTemplateProperty = AvaloniaProperty.Register<Tool, IDataTemplate?>(nameof(IconTemplate));
    
    public static readonly StyledProperty<BindingBase?> DescriptionBindingProperty = AvaloniaProperty.Register<Tool, BindingBase?>(nameof(DescriptionBinding));

    public static readonly StyledProperty<object?> CommandParameterProperty = AvaloniaProperty.Register<Tool, object?>(nameof(CommandParameter));
    
    public static readonly StyledProperty<Type?> DataTypeProperty = AvaloniaProperty.Register<Tool, Type?>(nameof(DataType), inherits: true);

    public static readonly DirectProperty<Tool, Classes> QuickAccessProperty 
        = AvaloniaProperty.RegisterDirect<Tool, Classes>(nameof(QuickAccess), tool => tool.QuickAccess);
    
    public static readonly DirectProperty<Tool, AvaloniaList<Tool>?> ChildToolsProperty 
        = AvaloniaProperty.RegisterDirect<Tool, AvaloniaList<Tool>?>(nameof(ChildTools), tool => tool.ChildTools);
    
    public string NameKey { get; set; } = string.Empty;

    public KeyGesture Gesture
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
    public BindingBase? DescriptionBinding
    {
        get => GetValue(DescriptionBindingProperty);
        set => SetValue(DescriptionBindingProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    [DataType]
    public Type? DataType
    {
        get => GetValue(DataTypeProperty);
        set => SetValue(DataTypeProperty, value);
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

    public AvaloniaList<Tool>? ChildTools { get; protected init; }

    public Classes QuickAccess { get; } = [];

    public event EventHandler? ResetKeybindingRequested;

    public event EventHandler<(string key, bool added)>? QuickAccessChanged;
    
    [RelayCommand]
    private void ToggleQuickAccessKey(string? key)
    {
        if (key is null) return;
        QuickAccess.Set(key, !QuickAccess.Contains(key));
        QuickAccessChanged?.Invoke(this, (key, QuickAccess.Contains(key)));
        RaisePropertyChanged(QuickAccessProperty, QuickAccess, QuickAccess);
    }
    
    public void ResetGesture()
    {
        ResetKeybindingRequested?.Invoke(this, EventArgs.Empty);
    }

    public void SetGestureToNone()
    {
        Gesture = new(Key.None);
    }
    
    public ToolInstance? TryFindTargetAndBuild(object? targetFinder) => targetFinder switch
    {
        not null when Build(targetFinder) is { } instance => instance,
        ContentPresenter contentPresenter when Build(contentPresenter.Content) is { } instance => instance,
        ContentControl contentControl when Build(contentControl.Content) is { } instance => instance,
        StyledElement styledElement when Build(styledElement.DataContext) is { } instance => instance,
        _ => null,
    };
    
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
    
    protected virtual BindingBase? GetCommandBinding() => null;

    public IEnumerator<Tool> GetEnumerator() => ChildTools?.GetEnumerator() ?? Enumerable.Empty<Tool>().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}