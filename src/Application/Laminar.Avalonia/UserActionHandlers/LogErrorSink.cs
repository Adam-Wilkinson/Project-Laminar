using Laminar.Contracts.Base.ActionSystem;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Laminar.Avalonia.UserActionHandlers;

internal class LogErrorSink(ILogger<LogErrorSink> logger) : IUnresolvedUserActionErrorSink
{
    public Task OnError(IUserAction action, UserActionError error)
    {
        logger.LogError(error.Exception, "An uncaught error occured when completing action {action}", action);
        return Task.CompletedTask;
    }
}
