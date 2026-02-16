using System.Reflection;

namespace Laminar.Avalonia.ViewModels.Services;

public interface IViewModelInitializer
{
    public void Initialize(ViewModelBase? parentViewModel, ViewModelBase viewModel, string viewModelName);
}