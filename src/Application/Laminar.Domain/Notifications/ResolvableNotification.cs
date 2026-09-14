namespace Laminar.Domain.Notifications;

public abstract class ResolvableNotification<TData, TParameter> : ResolvableNotification<TParameter>
{
    public abstract TData Data { get; }
}

public abstract class ResolvableNotification<T> : NotificationBase
{
    public void Resolve(T parameter)
    {
        ResolveOverride(parameter);
        DismissInternal();
    }
    
    protected abstract void ResolveOverride(T parameter);
}