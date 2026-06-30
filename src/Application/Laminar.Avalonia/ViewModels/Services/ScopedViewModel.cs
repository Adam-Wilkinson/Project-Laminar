using Laminar.Contracts.Base.ActionSystem;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Avalonia.ViewModels.Services;

public class ScopedViewModel<T> : IDisposable where T : ViewModelBase
{
    private readonly IServiceScope _scope;

    public T ViewModel { get; }

    public ScopedViewModel(IServiceProvider provider, Func<IServiceProvider, T> viewModelFactory)
    {
        _scope = provider.CreateScope();
        ViewModel = viewModelFactory(_scope.ServiceProvider);
        ViewModel.UserActionManager = _scope.ServiceProvider.GetRequiredService<IUserActionManager>();
    }
    
    public ScopedViewModel(IServiceProvider provider, params object[] constructorArgs) :
        this(provider, serviceProvider => ActivatorUtilities.CreateInstance<T>(serviceProvider, constructorArgs))
    {
    }
    
    public void Dispose()
    {
        _scope.Dispose();
        (ViewModel as IDisposable)?.Dispose();
        GC.SuppressFinalize(this);
    }
}