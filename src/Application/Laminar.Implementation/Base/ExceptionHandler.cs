using System.Runtime.ExceptionServices;
using Laminar.Contracts.Base;

namespace Laminar.Implementation.Base;

public class ExceptionHandler(IDispatcher dispatcher, IEnumerable<IExceptionSink> sinks) : IExceptionHandler
{
    public async Task OnExceptionAsync(Exception exception, CancellationToken cancellationToken = default)
    {
        foreach (var sink in sinks)
        {
            await sink.OnException(exception, cancellationToken);
        }
    }

    public async Task OnExceptionsAsync(IReadOnlyCollection<Exception> exceptions, CancellationToken cancellationToken = default)
    {
        foreach (var sink in sinks)
        {
            await sink.OnExceptions(exceptions, cancellationToken);
        }
    }

    public void OnException(Exception exception)
    {
        dispatcher.InvokeAsync(async void () =>
        {
            try
            {
                foreach (var sink in sinks)
                {
                    await sink.OnException(exception, CancellationToken.None);
                }
            }
            catch (Exception e)
            {
                ExceptionDispatchInfo.Capture(e).Throw();
            }
        });
    }
}