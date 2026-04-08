using System;
using HanumanInstitute.MvvmDialogs;
using Laminar.Contracts.UserData.FileNavigation;
using Laminar.Implementation.Extensions.ServiceInitializers;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Avalonia.ViewModels.Services;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddViewModels() => serviceCollection
            .AddDescendantsTransient<ViewModelBase>()
            .AddTransient<Func<ILaminarStorageItem, FileNavigatorItemViewModel>>(sp => item => ActivatorUtilities.CreateInstance<FileNavigatorItemViewModel>(sp, item))
            .AddDescendantsScoped<IViewModelInitializer>()
            .AddSingleton<IViewLocator, ViewLocator>();
    }
}