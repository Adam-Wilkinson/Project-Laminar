using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Laminar.Implementation;


namespace Laminar.Avalonia.Views;

public class AllScriptsViewer : UserControl
{
    private ToggleButton _addScriptButton;

    public AllScriptsViewer()
    {
        InitializeComponent();
        Instance dc = App.LaminarInstance;
        DataContext = dc;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _addScriptButton = this.FindControl<ToggleButton>("PART_AddScriptButton");
        _addScriptButton.GetObservable(ToggleButton.IsCheckedProperty).Subscribe(AddScriptButton_PointerPressed);
    }

    private void AddScriptButton_PointerPressed(bool? isChecked)
    {
        if (isChecked.HasValue && isChecked.Value)
        {
            MainWindow host = VisualRoot as MainWindow;

            if (host is not null && DataContext is Instance instance)
            {
                _addScriptButton.IsChecked = false;
                host.AddScript();
            }
        }
    }
}
