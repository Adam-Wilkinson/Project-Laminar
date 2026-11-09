using Laminar.Contracts.Base;
using Microsoft.Extensions.Logging;

namespace Laminar.Avalonia.Localizers;

internal class LogExceptionSink(ILogger<LogExceptionSink> logger) : IExceptionSink
{
    public Task OnException(Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An uncaught error occured");
        return Task.CompletedTask;
    }

    public Task OnExceptions(IReadOnlyCollection<Exception> exceptions, CancellationToken cancellationToken)
    {
        logger.LogError("A batch of {Count} exceptions was thrown", exceptions.Count);
        foreach (var exception in exceptions)
        {
            logger.LogError(exception, "An uncaught error occured");
        }

        return Task.CompletedTask;
    }
}
