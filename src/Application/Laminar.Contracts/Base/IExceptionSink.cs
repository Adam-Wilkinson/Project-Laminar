namespace Laminar.Contracts.Base;

public interface IExceptionSink
{
    public Task OnException(Exception exception);
}