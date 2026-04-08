using System;
using System.Collections.Generic;
using Avalonia.Controls;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Laminar.Avalonia.InitializationTargets;

namespace Laminar.Avalonia.ViewModels.Services;
public class ViewLocator(TopLevel topLevel) : ViewLocatorBase, IBeforeApplicationBuiltTarget
{
    private readonly Dictionary<object, Control> _allViewModelLogicalElements = []; 
    
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

    // public IActionScope? GetScope(object target)
    // {
    //     StyledElement? current = _allViewModelLogicalElements[target];
    //     while (current is not null)
    //     {
    //         if (current.DataContext is IActionScope actionScope)
    //         {
    //             return actionScope;
    //         }
    //
    //         current = current.Parent;
    //     }
    //
    //     return null;
    // }

    public override Control Build(object? viewModel)
    {
        var result = base.Build(viewModel);

        if (viewModel is not null)
        {
            _allViewModelLogicalElements[viewModel] = result;
        }

        return result;
    }
}
