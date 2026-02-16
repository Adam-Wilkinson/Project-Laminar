using HanumanInstitute.MvvmDialogs;
using Laminar.Implementation.Extensions.ServiceInitializers;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Avalonia.ViewModels.Services;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddViewModels() => serviceCollection
            .AddDescendantsTransient<ViewModelBase>()
            .AddDescendantsScoped<IViewModelInitializer>()
            .AddSingleton<IViewLocator, ViewLocator>();
    }
}