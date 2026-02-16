namespace Laminar.Avalonia.ViewModels.Services;

public class UndoScopeInitializer : IViewModelInitializer
{
    public void Initialize(ViewModelBase? parentViewModel, ViewModelBase viewModel, string viewModelName)
    {
        if (viewModel is IUndoScope undoScope)
        {
            viewModel.UndoScope = undoScope;
        }

        if (parentViewModel?.UndoScope is not null)
        {
            viewModel.UndoScope = parentViewModel.UndoScope;
        }
    }
}