using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace Laminar.Avalonia.ToolSystem;

public class ToolMenu : ItemsControl
{
    private static Flyout? _currentMenu;
    
    public static readonly AttachedProperty<Toolbox?> ContextToolboxProperty = AvaloniaProperty.RegisterAttached<ToolMenu, Visual, Toolbox?>("ContextToolbox");
    public static Toolbox? GetContextToolbox(Visual control) => control.GetValue(ContextToolboxProperty);
    public static void SetContextToolbox(Visual control, Toolbox? value) => control.SetValue(ContextToolboxProperty, value);
    
    public static readonly AttachedProperty<string?> QuickAccessKeyProperty = AvaloniaProperty.RegisterAttached<ToolMenu, Visual, string?>("QuickAccessKey");
    public static string? GetQuickAccessKey(Visual control) => control.GetValue(QuickAccessKeyProperty);
    public static void SetQuickAccessKey(Visual control, string? value) => control.SetValue(QuickAccessKeyProperty, value);
    
    static ToolMenu()
    {
        ContextRequestedEvent.AddClassHandler<Control>((control, e) =>
        {
            if (_currentMenu is not null)
            {
                _currentMenu.Hide();
                _currentMenu = null;
            }
            
            List<ToolMenuSection> allToolboxes = [];

            Visual current = control;
            while (current.GetVisualParent() is { } parent)
            {
                if (GetContextToolbox(current) is { } toolbox 
                    && toolbox.TryFindTargetAndBuild(current) is { ChildTools.Count: > 0 } instance)
                {
                    allToolboxes.Add(new ToolMenuSection(instance, GetQuickAccessKey(current)));
                }

                current = parent;
            }

            if (allToolboxes.Count == 0) return;

            var menu = new ToolMenu
            {
                ItemsSource = allToolboxes
            };

            Flyout generatedFlyout = new()
            {
                Content = menu,
                Placement = PlacementMode.Pointer,
            };
            generatedFlyout.FlyoutPresenterClasses.Add("no-border-flyout");
            
            generatedFlyout.ShowAt(control);
            e.Handled = true;
        });

        ContextCanceledEvent.AddClassHandler<Control>((_, e) =>
        {
            if (_currentMenu is null) return;
            _currentMenu?.Hide();
            _currentMenu = null;
            e.Handled = true;
        });
    }
}

public record class ToolMenuSection(ToolInstance ToolboxInstance, string? QuickAccessKey);