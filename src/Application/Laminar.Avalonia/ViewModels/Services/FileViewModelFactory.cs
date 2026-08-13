using Laminar.Contracts.Base;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.FileExplorer.Graph;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Storage.FileExplorer;
using Laminar.Implementation.Storage.PersistentData;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Avalonia.ViewModels.Services;

public sealed class OpenFile : IDisposable
{
    private readonly IDisposable _scope;

    private bool _disposed;
    
    public OpenFile(IFileResource<IEncodableDataOwner<IEncodableData>> fileResource,
        ViewModelBase fileViewModel,
        IDisposable scope)
    {
        _scope = scope;
        FileResource = fileResource;
        FileViewModel = fileViewModel;
        
        FileResource.Deleted += FileResourceOnDeleted;
    }

    public event EventHandler? Deleted;
    
    public IFileResource<IEncodableDataOwner<IEncodableData>> FileResource { get; }

    public ViewModelBase FileViewModel { get; }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        FileResource.Deleted -= FileResourceOnDeleted;
        _scope.Dispose();
        FileResource.Dispose();
        (FileViewModel as IDisposable)?.Dispose();
    }

    private void FileResourceOnDeleted(object? sender, EventArgs e)
    {
        Dispose();
        Deleted?.Invoke(this, EventArgs.Empty);
    }
}

public class FileViewModelFactory
{
    private readonly Dictionary<string, OpenFileFactory> _allFactories;
    private readonly IFileSystem _fileSystem;
    private readonly IExceptionHandler _exceptionHandler;
    private readonly IFileSystemGraph _fileSystemGraph;
    private readonly IServiceProvider _serviceProvider;
    
    private delegate OpenFile OpenFileFactory(IFileSystemFile file, IServiceProvider serviceProvider);
    
    public FileViewModelFactory(
        IFileSystem fileSystem,
        IExceptionHandler exceptionHandler,
        IFileSystemGraph fileSystemGraph,
        IServiceProvider serviceProvider)
    {
        _fileSystem = fileSystem;
        _exceptionHandler = exceptionHandler;
        _fileSystemGraph = fileSystemGraph;
        _serviceProvider = serviceProvider;
        _allFactories = new Dictionary<string, OpenFileFactory>
        {
            [FileSystemItemType.Script.Extension] = (file, sp) =>
            {
                var scriptingFactory = file.GetRootFolder().RuntimeHost.ScriptingFactory;
                var transcoder = new JsonPersistentDataTranscoder(null!);
                var scriptResource = file.GetContentsAsResource(transcoder, scriptingFactory);
                var scope = new ScopedViewModel<ScriptEditorViewModel>(sp,
                    scopedSp => ActivatorUtilities.CreateInstance<ScriptEditorViewModel>(scopedSp, scriptResource.Value));
                var viewModel = scope.ViewModel;
                return new OpenFile(scriptResource, viewModel, scope);
            }
        };
    }

    public async Task<OpenFile?> Open(FileSystemPath file, CancellationToken cancellationToken)
    {
        if (!_fileSystem.Exists(file))
        {
            await _exceptionHandler.OnExceptionAsync(new FileNotFoundException(file), cancellationToken);
            return null;
        }

        if (_fileSystem.IsDirectory(file))
        {
            await _exceptionHandler.OnExceptionAsync(new InvalidOperationException($"The path {file} is a directory and cannot be opened"), cancellationToken);
            return null;
        }

        var extension = _fileSystem.GetExtension(file);
        
        if (!_allFactories.TryGetValue(extension, out var openFileFactory))
        {
            await _exceptionHandler.OnExceptionAsync(new InvalidOperationException($"Unknown file format: {extension}"), cancellationToken);
            return null;
        }

        if (await _fileSystemGraph.GetOrLoad(file, cancellationToken) is not IFileSystemFile fileSystemFile)
        {
            await _exceptionHandler.OnExceptionAsync(new InvalidOperationException($"The file {file} is not loaded into memory"), cancellationToken);
            return null;
        }

        return openFileFactory(fileSystemFile, _serviceProvider);
    }
}