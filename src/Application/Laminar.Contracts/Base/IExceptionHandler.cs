namespace Laminar.Contracts.Base;

public interface IExceptionHandler
{
    public void OnException(Exception exception);
    Task OnExceptionAsync(Exception exception);
}