namespace Laminar.Contracts.Base;

public interface IExceptionSink
{
    public Task OnException(Exception exception, CancellationToken cancellationToken);
    
    public Task OnExceptions(IReadOnlyCollection<Exception> exceptions, CancellationToken cancellationToken);
}