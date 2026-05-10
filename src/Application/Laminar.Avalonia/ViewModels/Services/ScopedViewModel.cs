using System;
using Laminar.Contracts.Base.ActionSystem;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Avalonia.ViewModels.Services;

public class ScopedViewModel<T> : IDisposable where T : notnull
{
    private readonly IServiceScope _scope;

    public T ViewModel { get; }

    public ScopedViewModel(IServiceProvider provider, params object[] constructorArgs)
    {
        _scope = provider.CreateScope();
        ViewModel = ActivatorUtilities.CreateInstance<T>(_scope.ServiceProvider, constructorArgs);
        if (ViewModel is ViewModelBase vm)
        {
            vm.UserActionManager = _scope.ServiceProvider.GetRequiredService<IUserActionManager>();
        }
    }

    public void Dispose()
    {
        _scope.Dispose();
        GC.SuppressFinalize(this);
    }
}