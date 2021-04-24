using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Laminar_Avalonia.Views.CustomTitleBars
{
    public class MacosTitleBar : UserControl
    {
        private readonly Button closeButton;
        private readonly Button minimizeButton;
        private readonly Button zoomButton;

        private readonly DockPanel titleBarBackground;
        private readonly StackPanel titleAndWindowIconWrapper;

        public static readonly StyledProperty<bool> IsSeamlessProperty =
        AvaloniaProperty.Register<MacosTitleBar, bool>(nameof(IsSeamless));

        public bool IsSeamless
        {
            get { return GetValue(IsSeamlessProperty); }
            set {
                SetValue(IsSeamlessProperty, value);
                if (titleBarBackground != null && titleAndWindowIconWrapper != null)
                {
                    titleBarBackground.IsVisible = !IsSeamless;
                    titleAndWindowIconWrapper.IsVisible = !IsSeamless;
                }
            }
        }

        public MacosTitleBar()
        {
            InitializeComponent();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) == false)
            {
                IsVisible = false;
            }
            else
            {
                minimizeButton = this.FindControl<Button>("MinimizeButton");
                zoomButton = this.FindControl<Button>("ZoomButton");
                closeButton = this.FindControl<Button>("CloseButton");

                minimizeButton.Click += MinimizeWindow;
                zoomButton.Click += MaximizeWindow;
                closeButton.Click += CloseWindow;

                titleBarBackground = this.FindControl<DockPanel>("TitleBarBackground");
                titleAndWindowIconWrapper = this.FindControl<StackPanel>("TitleAndWindowIconWrapper");

                SubscribeToWindowState();
            }
        }

        private void CloseWindow(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Window hostWindow = (Window)VisualRoot;
            hostWindow.Close();
        }

        private void MaximizeWindow(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Window hostWindow = (Window)VisualRoot;

            if (hostWindow.WindowState == WindowState.Normal)
            {
                hostWindow.WindowState = WindowState.Maximized;
            }
            else
            {
                hostWindow.WindowState = WindowState.Normal;
            }
        }

        private void MinimizeWindow(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Window hostWindow = (Window)VisualRoot;
            hostWindow.WindowState = WindowState.Minimized;
        }

        private async void SubscribeToWindowState()
        {
            Window hostWindow = (Window)VisualRoot;

            while (hostWindow == null)
            {
                hostWindow = (Window)VisualRoot;
                await Task.Delay(50);
            }

            hostWindow.ExtendClientAreaTitleBarHeightHint = 44;
            hostWindow.GetObservable(Window.WindowStateProperty).Subscribe(s =>
            {
                if (s is not WindowState.Maximized)
                {
                    hostWindow.Padding = new Thickness(0, 0, 0, 0);
                }
                if (s is WindowState.Maximized)
                {
                    hostWindow.Padding = new Thickness(7, 7, 7, 7);

                    // This should be a more universal approach in both cases, but I found it to be less reliable, when for example double-clicking the title bar.
                    /*hostWindow.Padding = new Thickness(
                            hostWindow.OffScreenMargin.Left,
                            hostWindow.OffScreenMargin.Top,
                            hostWindow.OffScreenMargin.Right,
                            hostWindow.OffScreenMargin.Bottom);*/
                }
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
