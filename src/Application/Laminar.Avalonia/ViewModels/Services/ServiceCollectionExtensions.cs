using HanumanInstitute.MvvmDialogs;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Implementation.Extensions.ServiceInitializers;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Avalonia.ViewModels.Services;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddViewModels() => serviceCollection
            .AddDescendantsTransient<ViewModelBase>()
            .AddTransient<Func<IFileSystemItem, FileNavigatorItemViewModel>>(sp =>
                item => ActivatorUtilities.CreateInstance<FileNavigatorItemViewModel>(sp, item))
            .AddTransient<Func<FileSystemItemType, FileNavigatorItemViewModel>>(sp =>
                itemType => ActivatorUtilities.CreateInstance<FileNavigatorItemViewModel>(sp, itemType))
            .AddDescendantsScoped<IViewModelInitializer>()
            .AddSingleton<IViewLocator, ViewLocator>()
            .AddSingleton<FileExplorerLoadingQueue>()
            .AddSingleton<DialogService>()
            .AddSingleton<FileViewModelFactory>();
    }
}