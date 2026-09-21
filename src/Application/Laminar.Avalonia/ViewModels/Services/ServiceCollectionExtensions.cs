using HanumanInstitute.MvvmDialogs;
using Laminar.Avalonia.InitializationTargets;
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
            .AddSingleton<Func<IFileSystemItem, FileNavigatorItemViewModel>>(sp =>
                item => ActivatorUtilities.CreateInstance<FileNavigatorItemViewModel>(sp, item))
            .AddSingleton<Func<FileSystemItemType, FileNavigatorItemViewModel>>(sp =>
                itemType => ActivatorUtilities.CreateInstance<FileNavigatorItemViewModel>(sp, itemType))
            .AddDescendantsSingleton<IViewModelInitializer>()
            .AddSingleton<IViewLocator, ViewLocator>()
            .AddSingleton<FileExplorerLoadingQueue>()
            .AddSingleton<DialogService>()
            .AddSingleton<FocusedRuntimeManager>()
            .AddSingleton<FileViewModelFactory>();
    }
}