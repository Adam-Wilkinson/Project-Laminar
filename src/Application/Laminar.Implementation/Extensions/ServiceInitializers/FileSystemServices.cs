using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Infrastructure;
using Laminar.Contracts.Storage.IO;
using Laminar.Implementation.Storage.FileExplorer;
using Laminar.Implementation.Storage.FileExplorer.Infrastructure;
using Laminar.Implementation.Storage.IO;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Extensions.ServiceInitializers;

public static class FileSystemServicesExtension
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddFileSystemServices() => serviceCollection
            .AddSingleton<IFileSystem, FileSystem>()
            .AddSingleton<IFileSystemItemFactory, FileSystemItemFactory>()
            
            .AddSingleton<IFileSystemCommandService, FileSystemCommandService>()
            .AddSingleton<IFileSystemGraph, FileSystemGraph>()
            .AddSingleton<IFileSystemGraphMutator, FileSystemGraphMutator>()
            
            .AddSingleton<IFileSystemMonitor, FileSystemMonitor>()     
            .AddSingleton<IFileSystemMonitor, FileSystemMonitor>()
            .AddSingleton<IFileSystemMutationComputer, FileSystemMutationComputer>()
            .AddSingleton<IFileSystemItemHasher, FileSystemItemHasher>()
            .AddSingleton<IFileSystemSynchronizer, FileSystemSynchronizer>()
            .AddSingleton<IWritableFileSystemItemRepository, WritableFileSystemItemRepository>()
            .AddSingleton<IFileSystemItemRepository>(provider => provider.GetRequiredService<IWritableFileSystemItemRepository>())
            .AddSingleton<Func<IFileSystemEventHashBucket>>(provider => () => ActivatorUtilities.CreateInstance<FileSystemEventHashBucket>(provider))
            .AddSingleton<IFileSystemDiscrepancyComputer, FileSystemDiscrepancyComputer>()
            
            .AddScoped<IFileBrowser, FileBrowser>();
    }
}