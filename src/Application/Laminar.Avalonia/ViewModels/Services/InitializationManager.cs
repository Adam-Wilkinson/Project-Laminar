using System.Collections.Generic;
using Avalonia.Controls;
using Laminar.Avalonia.InitializationTargets;

namespace Laminar.Avalonia.ViewModels.Services;

public class InitializationManager(TopLevel topLevel, IEnumerable<IViewModelInitializer> initializers) 
    : IAfterApplicationBuiltTarget
{
    private readonly HashSet<ViewModelBase> _visitedViewModels = []; 
    
    public void OnApplicationBuilt()
    {
        if (topLevel.DataContext is ViewModelBase topLevelViewModel)
        {
            VisitViewModel(null, topLevelViewModel, string.Empty);
        }
    }

    private void VisitViewModel(ViewModelBase? parentViewModel, ViewModelBase viewModel, string viewModelName)
    {
        _visitedViewModels.Add(viewModel);
        foreach (var initializer in initializers)
        {
            initializer.Initialize(parentViewModel, viewModel, viewModelName);
        }
        
        foreach (var propertyInfo in viewModel.GetType().GetProperties())
        {
            if (propertyInfo.GetMethod is not null 
                && propertyInfo.PropertyType.IsSubclassOf(typeof(ViewModelBase))
                && propertyInfo.GetMethod.Invoke(viewModel, []) is ViewModelBase childViewModel
                && !_visitedViewModels.Contains(childViewModel))
            {
                VisitViewModel(viewModel, childViewModel, propertyInfo.Name);
            }
        }
    }
}