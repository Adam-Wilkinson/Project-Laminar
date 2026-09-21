using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Laminar.Domain.ValueObjects;

namespace Laminar.Avalonia.ViewModels;

public abstract class ViewModelBase : ObservableObject, IDisposable
{
    private readonly CompositeDisposable _disposables = new();
    private readonly INotifyPropertyChanged? _propertyChangedTarget;
    
    private bool _disposed;

    protected ViewModelBase()
    {
    }
    
    protected ViewModelBase(INotifyPropertyChanged propertyChangedTarget)
    {
        _propertyChangedTarget = propertyChangedTarget;
        _propertyChangedTarget.PropertyChanged += OnTargetPropertyChanged;
    }

    protected virtual void OnTargetPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
    }

    protected virtual void OnDisposed()
    {
    }

    protected void SubscribeToPropertyChanged(INotifyPropertyChanged target, string propertyName)
    {
        
    }
    
    protected T RegisterSubscription<T>(T disposable) where T : class, IDisposable
    {
        _disposables.Add(disposable);
        return disposable;
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _disposables.Dispose();
        _propertyChangedTarget?.PropertyChanged -= OnTargetPropertyChanged;
        OnDisposed();
        GC.SuppressFinalize(this);
    }
}