using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Reactive;

namespace Laminar.Avalonia.Views.CustomTitleBars;

public partial class WindowsTitleBar : UserControl
{
    public WindowsTitleBar()
    {
        InitializeComponent();
        IsVisible = !RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        MinimizeButton.Click += MinimizeWindow;
        MaximizeButton.Click += MaximizeWindow;
        CloseButton.Click += CloseWindow;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (VisualRoot is not Window hostWindow)
        {
            return;
        }

        hostWindow.GetObservable(Window.WindowStateProperty).Subscribe(new AnonymousObserver<WindowState>(s =>
        {
            if (s != WindowState.Maximized)
            {
                MaximizeIcon.Data = Geometry.Parse("M1,1 v-2 h-2 v2 z");
                hostWindow.Padding = new Thickness(0, 0, 0, 0);
                MaximizeToolTip.Content = "Maximize";
            }
            if (s == WindowState.Maximized)
            {
                MaximizeIcon.Data = Geometry.Parse("M2,1.5 h-0.5 v0.5 h0.5 z M2,1.8 h0.2 v-0.5 h-0.5 v0.2");
                hostWindow.Padding = new Thickness(7, 7, 7, 7);
                MaximizeToolTip.Content = "Restore Down";
            }
        }));
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
        if (VisualRoot is not Window hostWindow)
        {
            return;
        }

        hostWindow.WindowState = WindowState.Minimized;
    }
}
