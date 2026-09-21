using Laminar.Avalonia.ViewModels.Primitives;

namespace Laminar.Avalonia.ViewModels.Contracts;

public interface IViewModelInitializer
{
    public void Initialize(ViewModelBase? parentViewModel, ViewModelBase viewModel, string viewModelName);
}