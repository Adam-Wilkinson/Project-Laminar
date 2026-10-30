namespace Laminar.Contracts.Base;

public interface IExceptionHandler
{
    public void OnException(Exception exception);
    
    public Task OnExceptionAsync(Exception exception, CancellationToken cancellationToken = default);
    
    public Task OnExceptionsAsync(IEnumerable<Exception> exceptions, CancellationToken cancellationToken = default);
}