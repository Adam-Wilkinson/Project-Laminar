using System;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Avalonia.ViewModels.Services;

public class ScopedViewModel<T> : IDisposable where T : notnull
{
    private readonly IServiceScope _scope;

    public T ViewModel { get; }

    public ScopedViewModel(IServiceProvider provider)
    {
        _scope = provider.CreateScope();
        ViewModel = _scope.ServiceProvider.GetRequiredService<T>();   
    }

    public void Dispose()
    {
        _scope.Dispose();
        GC.SuppressFinalize(this);
    }
}