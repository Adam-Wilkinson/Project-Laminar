using System;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Laminar.Contracts.Base;
using Laminar.Domain;

namespace Laminar.Avalonia.UserActionHandlers;

internal class LogExceptionSink(ILogger<LogExceptionSink> logger) : IExceptionSink
{
    public Task OnException(Exception exception)
    {
        logger.LogError(exception, "An uncaught error occured");
        return Task.CompletedTask;
    }
}
