using System.Runtime.ExceptionServices;
using Laminar.Contracts.Base;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Storage.PersistentData;

internal class PersistentDataStore : IPersistentDataStore, IDisposable
{
    private static readonly TimeSpan FlushDelay = TimeSpan.FromMilliseconds(200);
    
    private readonly IFileContents _file;
    private readonly IPersistentDataTranscoder _transcoder;
    private readonly Timer _flushTimer;
    private readonly Lock _timerLock = new();
    
    private bool _isDisposed;
    
    public PersistentDataStore(
        IPersistentDataTranscoder persistentDataTranscoder,
        IFileContents file,
        IServiceProvider serviceProvider)
    {
        _file = file;
        _transcoder = persistentDataTranscoder;
        _flushTimer = new(_ => SynchronousFlush(), null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        Root = ActivatorUtilities.CreateInstance<PersistentDictionary>(serviceProvider);
        FileToDataNode();
        Root.OnInvalidated += OnChildValueInvalidated;
    }
    
    public void OnChildValueInvalidated(object? sender, EventArgs e)
    {
        if (_isDisposed) return;
        lock (_timerLock)
        {
            _flushTimer.Change(FlushDelay, Timeout.InfiniteTimeSpan);
        }
    }

    public IPersistentDictionary Root { get; }
    
    public void SynchronousFlush()
    {
        var encoded = Root.Encode(_transcoder);
        _file.Contents = _transcoder.ElementToBytes(encoded);
    }
    
    private void FileToDataNode()
    {
        if (_file.Contents.Length == 0)
        {
            return;
        }
        
        var decoded = _transcoder.BytesToElement(_file.Contents) ?? throw new InvalidOperationException();
        Root.Decode(_transcoder, decoded);
    }

    public void Dispose()
    {
        _file.Dispose();
        Root.OnInvalidated -= OnChildValueInvalidated;
        _flushTimer.Dispose();
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }
}