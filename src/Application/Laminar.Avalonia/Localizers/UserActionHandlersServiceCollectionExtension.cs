using Laminar.Contracts.Base;
using Laminar.Contracts.Base.ActionSystem;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Avalonia.Localizers;

internal static class UserActionHandlersServiceCollectionExtension
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection AddLocalizers() => collection
            .AddSingleton<IUserActionErrorResolver, UserPromptErrorResolver>()
            .AddSingleton<IExceptionSink, UserPromptExceptionSink>()
            .AddSingleton<IExceptionSink, LogExceptionSink>();
    }
}
