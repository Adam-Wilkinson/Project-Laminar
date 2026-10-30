using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Laminar.Contracts.Workspaces;

namespace Laminar.Avalonia.ViewModels.Services;

public class FocusedWorkspaceManager
{
    public const string ResourceKey = "FocusedWorkspaceManager";
    
    public static readonly AttachedProperty<IWorkspace?> FocusWorkspaceProperty 
        = AvaloniaProperty.RegisterAttached<FocusedWorkspaceManager, InputElement, IWorkspace?>("FocusWorkspace", inherits: true);
    public static IWorkspace? GetFocusWorkspace(InputElement element) => element.GetValue(FocusWorkspaceProperty);
    public static void SetFocusWorkspace(InputElement element, IWorkspace? value) => element.SetValue(FocusWorkspaceProperty, value);
    
    static FocusedWorkspaceManager()
    {
        FocusWorkspaceProperty.Changed.AddClassHandler<InputElement>(FocusWorkspaceChanged);
    }

    private static void FocusWorkspaceChanged(InputElement element, AvaloniaPropertyChangedEventArgs args)
    {
        var newWorkspace = args.GetNewValue<IWorkspace?>();

        if (newWorkspace is null)
        {
            element.GotFocus -= ElementOnGotFocus;
        }
        else
        {
            element.GotFocus += ElementOnGotFocus;
        }
    }

    private static void ElementOnGotFocus(object? sender, FocusChangedEventArgs e)
    {
        if (e.NewFocusedElement is not InputElement focusedElement) return;
        if (GetFocusWorkspace(focusedElement) is not { } workspace) return;
        if (!focusedElement.TryFindResource(ResourceKey, out var resource) 
            || resource is not FocusedWorkspaceManager instance) return;
        
        instance.Focus(workspace);
    }
    
    public void Focus(IWorkspace? host)
    {
        if (Equals(host, FocusedWorkspace)) return;
        Console.WriteLine($"New focused workspace: {host?.Name}");
        FocusedWorkspace = host;
        FocusedWorkspaceChanged?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? FocusedWorkspaceChanged;
    
    public IWorkspace? FocusedWorkspace { get; private set; }
}