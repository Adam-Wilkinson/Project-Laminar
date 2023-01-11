using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Laminar.Avalonia.KeyBIndings;
using Laminar.Avalonia.Models;
using Laminar.Contracts.ActionSystem;
using Laminar.Contracts.NodeSystem;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Laminar.Avalonia.Views;

public class MainWindow : Window
{
    private bool _needsScriptInstance = false;
    private readonly IScriptFactory _scriptFactory = App.LaminarInstance.ServiceProvider.GetService<IScriptFactory>();

    public MainWindow()
    {
        KeyBindings.Add(new LaminarKeyBinding("Undo", new KeyGesture(Key.Z, KeyModifiers.Control), () => App.LaminarInstance.ServiceProvider.GetRequiredService<IUserActionManager>().Undo()));
        KeyBindings.Add(new LaminarKeyBinding("Redo", new KeyGesture(Key.Z, KeyModifiers.Control | KeyModifiers.Shift), () => App.LaminarInstance.ServiceProvider.GetRequiredService<IUserActionManager>().Redo()));
        KeyBindings.Add(new LaminarKeyBinding("RedoAlt", new KeyGesture(Key.Y, KeyModifiers.Control), () => App.LaminarInstance.ServiceProvider.GetRequiredService<IUserActionManager>().Redo()));
        FontFamily = new FontFamily("Lucida Sans");
        InitializeComponent();
        DataContext = new MainWindowViewModel();
#if DEBUG
        // this.AttachDevTools();
#endif
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) == true)
        {
            UseNativeTitleBar();
        }

        Resources["HeaderColour"] = new SolidColorBrush(new Color(255, 19, 19, 35));
    }

    public void OpenScriptEditor(IScript script)
    {
        (DataContext as MainWindowViewModel).ShowScriptEditor(script);
    }

    public void CloseScriptEditor()
    {
        if (DataContext is MainWindowViewModel mwvm && mwvm.MainControl is ScriptEditor scriptEditor && scriptEditor.DataContext is IScript openNodeTree)
        {
            openNodeTree.ExecutionInstance.IsShownInUI = false;
            if (_needsScriptInstance)
            {
                AddScriptInstance(openNodeTree);
                _needsScriptInstance = false;
            }
            mwvm.ShowAllScripts();
        }
    }

    public void AddScriptInstance(IScript script)
    {
        (DataContext as MainWindowViewModel).CloseAddScriptsButton();
    }

    public void AddScript()
    {
        IScript newScript = _scriptFactory.CreateScript();

        newScript.Name = "Untitled Script";

        OpenScriptEditor(newScript);
    }

    protected override void OnClosed(EventArgs e)
    {

    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void UseNativeTitleBar()
    {
        ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.SystemChrome;
        ExtendClientAreaTitleBarHeightHint = -1;
        ExtendClientAreaToDecorationsHint = false;
    }
}
