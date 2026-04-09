using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Media.Transformation;
using Avalonia.Reactive;

namespace Laminar.Avalonia.Views;
public partial class MainWindow : Window
{
    public static readonly StyledProperty<TransformOperations> HiddenSettingsRenderTransformProperty = AvaloniaProperty.Register<MainWindow, TransformOperations>(nameof(HiddenSettingsRenderTransform));

    public TransformOperations HiddenSettingsRenderTransform
    {
        get => GetValue(HiddenSettingsRenderTransformProperty);
        set => SetValue(HiddenSettingsRenderTransformProperty, value);
    }

    public MainWindow()
    {
        InitializeComponent();

        SettingsMenu.GetObservable(BoundsProperty).Subscribe(new AnonymousObserver<Rect>(SettingsBoundsChanged));
        SettingsBoundsChanged(SettingsMenu.Bounds);
    }

    private void SettingsBoundsChanged(Rect newBounds)
    {
        var operations = TransformOperations.CreateBuilder(1);
        operations.AppendTranslate(0, -newBounds.Height);
        HiddenSettingsRenderTransform = operations.Build();
    }
}