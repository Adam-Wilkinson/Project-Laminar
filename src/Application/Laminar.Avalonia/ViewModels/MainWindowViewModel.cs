using ReactiveUI;
using System.Threading;
using Laminar.Contracts.Scripting;
using Laminar.Implementation;
using Laminar.PluginFramework.Registration;
using Microsoft.Extensions.DependencyInjection;
using Laminar.Contracts.Scripting.NodeWrapping;

namespace Laminar.Avalonia.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IScriptFactory _scriptFactory;

    private ViewModelBase _content;

    public MainWindowViewModel()
    {
        _scriptFactory = LaminarInstance.ServiceProvider.GetService<IScriptFactory>();
        ShowHomepage();
    }

    public Instance LaminarInstance { get; } = new Instance(SynchronizationContext.Current, FrontendDependency.Avalonia);

    public HomepageViewModel HomepageViewModel { get; }

    public ViewModelBase Content
    {
        get => _content;
        set => this.RaiseAndSetIfChanged(ref _content, value);
    }

    public void AddScript()
    {
        IScript newScript = _scriptFactory.CreateScript();
        newScript.Name = "Untitled Script";
        LaminarInstance.AllScripts.Add(newScript);
        OpenScriptEditor(newScript);
    }

    public void OpenScriptEditor(IScript script)
    {
        Content = new ScriptEditorViewModel(script, LaminarInstance.ServiceProvider.GetService<ILoadedNodeManager>());
    }

    public void ShowHomepage()
    {
        Content = new HomepageViewModel
        {
            AllScripts = LaminarInstance.AllScripts,
        };
    }
}
