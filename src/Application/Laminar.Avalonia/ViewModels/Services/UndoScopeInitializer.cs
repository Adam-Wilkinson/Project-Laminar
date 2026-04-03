using Laminar.Contracts.Base.ActionSystem;

namespace Laminar.Avalonia.ViewModels.Services;

public class UndoScopeInitializer(IUserActionManager userActionManager) : IViewModelInitializer
{
    public void Initialize(ViewModelBase? parentViewModel, ViewModelBase viewModel, string viewModelName)
    {
        if (viewModel is IActionScope undoScope)
        {
            viewModel.UndoScope = undoScope;
        }
        else if (parentViewModel?.UndoScope is not null)
        {
            viewModel.UndoScope = parentViewModel.UndoScope;
        }
        
        viewModel.UserActionManager = userActionManager;
    }
}