using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Laminar.Contracts.Base.PluginLoading;

namespace Laminar.Avalonia.ViewModels.Services;

public class FocusedRuntimeManager
{
    public const string ResourceKey = "FocusedRuntimeManager";
    
    public static readonly AttachedProperty<IRuntimeHost?> FocusRuntimeProperty 
        = AvaloniaProperty.RegisterAttached<FocusedRuntimeManager, InputElement, IRuntimeHost?>("FocusRuntime", inherits: true);
    public static IRuntimeHost? GetFocusRuntime(InputElement element) => element.GetValue(FocusRuntimeProperty);
    public static void SetFocusRuntime(InputElement element, IRuntimeHost? value) => element.SetValue(FocusRuntimeProperty, value);
    
    static FocusedRuntimeManager()
    {
        FocusRuntimeProperty.Changed.AddClassHandler<InputElement>(FocusRuntimeChanged);
    }

    private static void FocusRuntimeChanged(InputElement element, AvaloniaPropertyChangedEventArgs args)
    {
        var newRuntime = args.GetNewValue<IRuntimeHost?>();

        if (newRuntime is null)
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
        if (GetFocusRuntime(focusedElement) is not { } runtimeHost) return;
        if (!focusedElement.TryFindResource(ResourceKey, out var resource) 
            || resource is not FocusedRuntimeManager instance) return;
        
        instance.Focus(runtimeHost);
    }
    
    public void Focus(IRuntimeHost? host)
    {
        if (Equals(host, FocusedRuntime)) return;
        Console.WriteLine($"New focused runtime: {host?.Name}");
        FocusedRuntime = host;
        FocusedRuntimeChanged?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? FocusedRuntimeChanged;
    
    public IRuntimeHost? FocusedRuntime { get; private set; }
}