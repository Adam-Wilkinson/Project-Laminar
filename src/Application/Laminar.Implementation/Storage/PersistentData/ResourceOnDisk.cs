using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Storage.PersistentData;

public sealed class ResourceOnDisk<TValue, TData> 
    : IResourceOnDisk<TValue> 
    where TValue : class, IEncodableDataOwner<TData> where TData : class, IEncodableData
{
    private static readonly TimeSpan FlushDelay = TimeSpan.FromMilliseconds(200);
    
    private readonly IFileSystem _fileSystem;
    private readonly Timer _flushTimer;
    private readonly Lock _timerLock = new();
    
    private IPersistentDataTranscoder _transcoder;
    private IFileContents _fileContents;
    private bool _isDisposed;

    public ResourceOnDisk(
        FileSystemPath filePath,
        IPersistentDataTranscoder transcoder,
        IFileSystem fileSystem,
        IEncodableDataFactory dataFactory,
        IDecodingFactory<TValue, TData> decodingFactory)
    {
        _transcoder = transcoder;
        _fileContents = fileSystem.GetFile(filePath);
        _fileSystem = fileSystem;
        _flushTimer = new(_ => SynchronousFlush(), null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        
        var data = dataFactory.GetEncodableData<TData>();
        if (_fileContents.Contents.Length > 0)
        {
            var decoded = _transcoder.BytesToElement(_fileContents.Contents) ?? throw new InvalidOperationException();
            data.Decode(_transcoder, decoded);
        }
        
        data.Invalidated += OnDataInvalidated;
        Resource = decodingFactory.FromPersistentData(data);
    }

    public TValue Resource { get; }

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