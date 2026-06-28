using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Storage.PersistentData;

public sealed class FileSyncedResource<T> : IFileSyncedResource<T> where T : class, IEncodableDataOwner
{
    private static readonly TimeSpan FlushDelay = TimeSpan.FromMilliseconds(200);
    
    private readonly IFileSystem _fileSystem;
    private readonly Timer _flushTimer;
    private readonly Lock _timerLock = new();
    
    private IPersistentDataTranscoder _transcoder;
    private IFileContents _fileContents;
    private bool _isDisposed;

    public FileSyncedResource(
        T resource, 
        FileSystemPath filePath,
        IPersistentDataTranscoder transcoder,
        IFileSystem fileSystem)
    {
        Resource = resource;
        _transcoder = transcoder;
        _fileContents = fileSystem.GetFile(filePath);
        _fileSystem = fileSystem;
        _flushTimer = new(_ => SynchronousFlush(), null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        
        if (_fileContents.Contents.Length > 0)
        {
            var decoded = _transcoder.BytesToElement(_fileContents.Contents) ?? throw new InvalidOperationException();
            Resource.Data.Decode(_transcoder, decoded);
        }
        
        Resource.Data.Invalidated += OnDataInvalidated;
    }

    public T Resource { get; }

    public IPersistentDataTranscoder Transcoder
    {
        get => _transcoder;
        set
        {
            _transcoder = value;
            OnDataInvalidated(null, EventArgs.Empty);
        }
    }

    public FileSystemPath Location
    {
        get => _fileContents.Path;
        set
        {
            _fileContents.Dispose();
            _fileContents = _fileSystem.GetFile(value);
            OnDataInvalidated(null, EventArgs.Empty);
        }
    }

    public event EventHandler? OnDisposed;

    internal void SynchronousFlush()
    {
        var encoded = Resource.Data.Encode(_transcoder);
        _fileContents.Contents = _transcoder.ElementToBytes(encoded);
    }

    private void OnDataInvalidated(object? sender, EventArgs e)
    {
        if (_isDisposed) return;
        lock (_timerLock)
        {
            _flushTimer.Change(FlushDelay, Timeout.InfiniteTimeSpan);
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _fileContents.Dispose();
        Resource.Data.Invalidated -= OnDataInvalidated;
        lock (_timerLock)
        {
            _flushTimer.Change(FlushDelay, Timeout.InfiniteTimeSpan);
        }
        
        OnDisposed?.Invoke(this, EventArgs.Empty);
    }
}