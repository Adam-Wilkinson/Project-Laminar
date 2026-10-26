using CommunityToolkit.Mvvm.Input;

namespace Laminar.Avalonia.ViewModels.Contracts;

public interface IUninstallable
{
    public IRelayCommand UninstallCommand { get; }
}