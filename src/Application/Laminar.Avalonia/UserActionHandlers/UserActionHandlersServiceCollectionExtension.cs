using Laminar.Contracts.Base.ActionSystem;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Avalonia.UserActionHandlers;

internal static class UserActionHandlersServiceCollectionExtension
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection AddUserActionHandlers() => collection
            .AddSingleton<IUserActionErrorResolver, UserPromptErrorResolver>()
            .AddSingleton<IUnresolvedUserActionErrorSink, UserPromptErrorSink>()
            .AddSingleton<IUnresolvedUserActionErrorSink, LogErrorSink>();
    }
}
