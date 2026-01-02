using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;

namespace Laminar.Avalonia.ViewModels.Services;
public class ViewLocator : ViewLocatorBase
{
    protected override string GetViewName(object viewModel)
    {
        return viewModel.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
    }

    public override bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
