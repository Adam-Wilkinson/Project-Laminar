using Avalonia.Controls;
using Avalonia.Interactivity;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Laminar.Avalonia.ViewModels;

namespace Laminar.Avalonia.InitializationTargets;

public class ViewLocator(TopLevel topLevel) : ViewLocatorBase, IBeforeApplicationBuiltTarget
{
    protected override string GetViewName(object viewModel)
    {
        return viewModel.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
    }

    public override bool Match(object? data)
    {
        return data is ViewModelBase;
    }

    public void BeforeApplicationBuiltInitialization()
    {
        topLevel.DataTemplates.Add(this);
    }

    public override Control Build(object? viewModel)
    {
        var result = base.Build(viewModel);

        if (viewModel is not ViewModelBase vmBase) return result;
        
        result.Unloaded += ResultOnUnloaded;

        return result;

        void ResultOnUnloaded(object? sender, RoutedEventArgs e)
        {
            vmBase.Dispose();
            result.Unloaded -= ResultOnUnloaded;
        }
    }
}
