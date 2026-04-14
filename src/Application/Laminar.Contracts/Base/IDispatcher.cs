namespace Laminar.Contracts.Base;

public interface IDispatcher
{
    public Task InvokeAsync(Action action);
}
