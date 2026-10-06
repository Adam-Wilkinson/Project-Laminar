namespace Laminar.Domain.ValueObjects;

public sealed class FuncDisposable(Action onDispose) : IDisposable
{
    public void Dispose() => onDispose();
}