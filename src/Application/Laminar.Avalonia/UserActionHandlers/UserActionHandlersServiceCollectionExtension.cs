using Laminar.Contracts.Base;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Avalonia.UserActionHandlers;

internal static class UserActionHandlersServiceCollectionExtension
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection AddUserActionHandlers() => collection
            .AddSingleton<IUserActionErrorResolver, UserPromptErrorResolver>()
            .AddSingleton<IExceptionSink, UserPromptExceptionSink>()
            .AddSingleton<IExceptionSink, LogExceptionSink>();
    }
}
