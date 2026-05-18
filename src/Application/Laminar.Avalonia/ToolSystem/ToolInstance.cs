using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;

namespace Laminar.Avalonia.ToolSystem;

public class ToolInstance : StyledElement, IEnumerable<ToolInstance>
{
    public static readonly StyledProperty<ICommand> CommandProperty = AvaloniaProperty.Register<ToolInstance, ICommand>(nameof(Command), defaultValue: DefaultCommand.Instance);
    
    public static readonly StyledProperty<string> DescriptionProperty = AvaloniaProperty.Register<ToolInstance, string>(nameof(Description));
    
    public static readonly StyledProperty<Tool> ToolProperty = AvaloniaProperty.Register<ToolInstance, Tool>(nameof(Tool));

    // public static readonly StyledProperty<List<ToolInstance>?> ChildToolsProperty = AvaloniaProperty.Register<ToolInstance, List<ToolInstance>?>(nameof(ChildTools), defaultValue: []);
    
    static ToolInstance()
    {
        ToolProperty.Changed.AddClassHandler<ToolInstance>((o, e) => o.ToolChanged(e));
    }

    private void ToolChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var (oldValue, newValue) = e.GetOldAndNewValue<Tool?>();
        oldValue?.ChildTools?.CollectionChanged -= ToolCollectionChanged;
        newValue?.ChildTools?.CollectionChanged += ToolCollectionChanged;
        ToolCollectionChanged(null, null);
    }

    private void ToolCollectionChanged(object? sender, NotifyCollectionChangedEventArgs? e)
    {
        ChildTools?.Clear();
        if (Tool.ChildTools is not { } newToolChildren) return;
        
        ChildTools?.AddRange(newToolChildren.Select(x =>
        {
            var childTool = x.Build(DataContext);
            (childTool as ISetInheritanceParent)?.SetParent(this);
            return childTool;
        }).OfType<ToolInstance>());
    }

    public AvaloniaList<ToolInstance>? ChildTools { get; } = [];

    public ICommand Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
    
    public Tool Tool
    {
        get => GetValue(ToolProperty);
        set => SetValue(ToolProperty, value);
    }


    public Visual? PopupTarget { get; set; }

    public IEnumerator<ToolInstance> GetEnumerator() =>
        ChildTools?.GetEnumerator() ?? Enumerable.Empty<ToolInstance>().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}