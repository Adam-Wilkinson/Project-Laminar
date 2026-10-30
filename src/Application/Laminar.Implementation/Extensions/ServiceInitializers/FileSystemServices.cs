using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Graph;
using Laminar.Contracts.Storage.FileExplorer.Synchronization;
using Laminar.Contracts.Storage.IO;
using Laminar.Implementation.Storage.FileExplorer;
using Laminar.Implementation.Storage.FileExplorer.Graph;
using Laminar.Implementation.Storage.FileExplorer.Synchronization;
using Laminar.Implementation.Storage.IO;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Extensions.ServiceInitializers;

public static class FileSystemServicesExtension
{
    public static IServiceCollection AddFileSystemServices(this IServiceCollection services) => services
        .AddSingleton<IFileSystem, FileSystem>()
        .AddSingleton<IFileSystemItemFactory, FileSystemItemFactory>()
        .AddSingleton<IFileResourceFactory, FileResourceFactory>()
            
        .AddSingleton<IFileSystemCommandService, FileSystemCommandService>()
        .AddSingleton<IFileSystemGraph, FileSystemGraph>()
        .AddSingleton<IGraphMutationApplier, GraphMutationApplier>()
            
        .AddSingleton<IFileSystemMonitor, FileSystemMonitor>()
        .AddSingleton<IFileSystemMutationComputer, FileSystemMutationComputer>()
        .AddSingleton<IFileSystemItemHasher, FileSystemItemHasher>()
        .AddSingleton<IFileSystemSynchronizer, FileSystemSynchronizer>()
        .AddSingleton<IMutableFileSystemItemRepository, FileSystemItemRepository>()
        .AddSingleton<IFileSystemItemRepository>(provider => provider.GetRequiredService<IMutableFileSystemItemRepository>())
        .AddFactory<IOutdatedItemsBuffer?, IFileSystemEventHashBucket, FileSystemEventHashBucket>()
        .AddSingleton<IFileSystemDiscrepancyComputer, FileSystemDiscrepancyComputer>()
            
        .AddScoped<IFileBrowser, FileBrowser>();
}