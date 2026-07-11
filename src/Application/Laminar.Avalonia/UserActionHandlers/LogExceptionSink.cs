using Microsoft.Extensions.Logging;
using Laminar.Contracts.Base;

namespace Laminar.Avalonia.UserActionHandlers;

internal class LogExceptionSink(ILogger<LogExceptionSink> logger) : IExceptionSink
{
    public Task OnException(Exception exception)
    {
        logger.LogError(exception, "An uncaught error occured");
        return Task.CompletedTask;
    }
}
