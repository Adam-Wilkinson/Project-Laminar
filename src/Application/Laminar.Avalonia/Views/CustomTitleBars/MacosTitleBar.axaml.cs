using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Reactive;

namespace Laminar.Avalonia.Views.CustomTitleBars;

public partial class MacosTitleBar : UserControl
{
    public MacosTitleBar()
    {
        InitializeComponent();
        IsVisible = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        MinimizeButton.Click += MinimizeWindow;
        ZoomButton.Click += MaximizeWindow;
        CloseButton.Click += CloseWindow;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (VisualRoot is not Window hostWindow)
        {
            return;
        }

        hostWindow.ExtendClientAreaTitleBarHeightHint = 44;
        hostWindow.GetObservable(Window.WindowStateProperty).Subscribe(new AnonymousObserver<WindowState>(s => 
            hostWindow.Padding = s switch
            {
                WindowState.Maximized => new Thickness(7, 7, 7, 7),
                _ => new Thickness(0, 0, 0, 0)
            }
        ));
    }

    private void CloseWindow(object? sender, RoutedEventArgs e)
    {
        ((Window)VisualRoot)?.Close();
    }

    private void MaximizeWindow(object? sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window hostWindow)
        {
            return;
        }

        if (hostWindow.WindowState == WindowState.Normal)
        {
            hostWindow.WindowState = WindowState.Maximized;
        }
        else
        {
            hostWindow.WindowState = WindowState.Normal;
        }
    }

    private void MinimizeWindow(object? sender, RoutedEventArgs e)
    {
        if (VisualRoot is Window hostWindow)
        {
            hostWindow.WindowState |= WindowState.Minimized;
        }
    }
}
