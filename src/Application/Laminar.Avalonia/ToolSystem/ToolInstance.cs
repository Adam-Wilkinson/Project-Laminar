using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace Laminar.Avalonia.ToolSystem;

public class ToolInstance : StyledElement, IEnumerable<ToolInstance>
{
    public static readonly StyledProperty<ICommand> CommandProperty = AvaloniaProperty.Register<ToolInstance, ICommand>(nameof(Command), defaultValue: DefaultCommand.Instance);
    
    public static readonly StyledProperty<string> DescriptionProperty = AvaloniaProperty.Register<ToolInstance, string>(nameof(Description));
    
    public static readonly StyledProperty<Tool> ToolTemplateProperty = AvaloniaProperty.Register<ToolInstance, Tool>(nameof(Tool));

    public static readonly StyledProperty<List<ToolInstance>> ChildToolsProperty = AvaloniaProperty.Register<ToolInstance, List<ToolInstance>>(nameof(ChildTools), defaultValue: []);
    
    static ToolInstance()
    {
        ToolTemplateProperty.Changed.AddClassHandler<ToolInstance>((o, e) => o.TemplateChanged(e));
    }

    private void TemplateChanged(AvaloniaPropertyChangedEventArgs _)
    {
        ChildTools = Tool.ChildTools.Select(x =>
        {
            var childTool = x.Build(DataContext);
            ((ISetInheritanceParent)childTool)?.SetParent(this);
            return childTool;
        }).OfType<ToolInstance>().ToList();
    }

    public List<ToolInstance> ChildTools
    {
        get => GetValue(ChildToolsProperty);
        set => SetValue(ChildToolsProperty, value);
    }
    
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
        get => GetValue(ToolTemplateProperty);
        set => SetValue(ToolTemplateProperty, value);
    }

    public IEnumerator<ToolInstance> GetEnumerator() => ChildTools.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}