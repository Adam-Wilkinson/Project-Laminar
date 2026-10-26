using CommunityToolkit.Mvvm.Input;
using Laminar.Avalonia.ViewModels.Primitives;

namespace Laminar.Avalonia.ViewModels.Contracts;

public interface ISetVersion
{
    public RelativeVersionState SetVersionType { get; }

    public IObservable<string> SetVersionDescription { get; }
    
    public IAsyncRelayCommand SetVersionCommand { get; }
}