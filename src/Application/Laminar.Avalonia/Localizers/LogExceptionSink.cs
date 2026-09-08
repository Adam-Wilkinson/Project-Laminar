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
}
