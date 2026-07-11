using System.ComponentModel;
using Laminar.Contracts.Base;
using Laminar.Contracts.Storage.FileExplorer;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.FileExplorer;

internal sealed class FileResource<TValue, TData> : IFileResource<TValue> 
    where TValue : class, IEncodableDataOwner<IEncodableData>, IEncodableDataOwner<TData>
    where TData : class, IEncodableData
{
    private readonly IFileSystem _fileSystem;
    private readonly IPersistentDataManager _persistentDataManager;
    private readonly IPersistentDataTranscoder _transcoder;
    private readonly IExceptionHandler _exceptionHandler;
    
    private bool _isDisposed;
    private IDataOnDisk<TData>? _dataOnDisk;
    private IFileWaiter? _fileCreationWaiter;
    
    public FileResource(
        FileSystemFile file,
        IPersistentDataTranscoder transcoder, 
        IDecodingFactory<TValue, TData> factory, 
        IPersistentDataManager dataManager, 
        IFileSystem fileSystem,
        IExceptionHandler exceptionHandler)
    {
        _fileSystem = fileSystem;
        _transcoder = transcoder;
        _persistentDataManager = dataManager;
        _exceptionHandler = exceptionHandler;
        
        _dataOnDisk = dataManager.GetDataOnDisk<TData>(file.Path, transcoder);

        Value = factory.FromPersistentData(_dataOnDisk.Data);
        
        File = file;
        File.PropertyChanged += OnFilePropertyChanged;
        File.OnDeleted += OnFileDeleted;
    }

    public IFileSystemFile File { get; }

    public TValue Value { get; }

    public event EventHandler? OnDeleted;

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        CleanupDiskResource();
        File.PropertyChanged -= OnFilePropertyChanged;
        File.OnDeleted -= OnFileDeleted;
    }

    private void OnFileDeleted(object? sender, EventArgs e)
    {
        Dispose();
        OnDeleted?.Invoke(this, EventArgs.Empty);
    }

    private void OnFilePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isDisposed || e.PropertyName != nameof(IFileSystemItem.Path)) return;

        InitializeOrAwaitDiskResource();
    }

    private void InitializeOrAwaitDiskResource()
    {
        if (_isDisposed) return;
        CleanupDiskResource();
        
        if (_fileSystem.Exists(File.Path))
        {
            _dataOnDisk = _persistentDataManager.GetDataOnDisk(File.Path, _transcoder, ((IEncodableDataOwner<TData>)Value).Data);
            return;
        }

        _fileCreationWaiter = _fileSystem.GetFileWaiter(File.Path, TimeSpan.FromSeconds(2));
        _fileCreationWaiter.FileCreated += FileCreated;
        _fileCreationWaiter.WaitWarning += FileCreationWaitWarning;
    }

    private void FileCreationWaitWarning(object? sender, EventArgs e)
    {
        _exceptionHandler.OnException(new FileCreationTimeoutException(File.Path));
        File.GetRootFolder().Refresh();
    }

    private void FileCreated(object? sender, EventArgs e) => InitializeOrAwaitDiskResource();

    private void CleanupDiskResource()
    {
        _dataOnDisk?.Dispose();
        _dataOnDisk = null;
        _fileCreationWaiter?.FileCreated -= FileCreated;
        _fileCreationWaiter?.WaitWarning -= FileCreationWaitWarning;
        _fileCreationWaiter?.Dispose();
        _fileCreationWaiter = null;
    }
}