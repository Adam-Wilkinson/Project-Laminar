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
    private readonly IDispatcher _dispatcher;
    private readonly IPersistentDataTranscoder _transcoder;
    
    private CancellationTokenSource? _flushCts;
    private bool _isDisposed;
    
    public PersistentDataStore(
        IPersistentDataTranscoder persistentDataTranscoder,
        IFileContents file,
        IServiceProvider serviceProvider,
        IDispatcher dispatcher)
    {
        _file = file;
        _dispatcher = dispatcher;
        _transcoder = persistentDataTranscoder;
        Root = ActivatorUtilities.CreateInstance<PersistentDictionary>(serviceProvider);
        Root.OnInvalidated += OnChildValueInvalidated;
        FileToDataNode();
    }
    
    public void OnChildValueInvalidated(object? sender, EventArgs e)
    {
        if (_isDisposed) return;
        ScheduleFlush();
    }

    public IPersistentDictionary Root { get; }
    
    private void ScheduleFlush()
    {
        _flushCts?.Cancel();
        _flushCts = new CancellationTokenSource();

        CancellationToken token = _flushCts.Token;

        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(FlushDelay, token);
                SynchronousFlush();
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                await _dispatcher.InvokeAsync(() => ExceptionDispatchInfo.Capture(ex).Throw());
            }
        }, token);
    }

    public void SynchronousFlush()
    {
        _flushCts?.Cancel();
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
        _flushCts?.Dispose();
        Root.OnInvalidated -= OnChildValueInvalidated;
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }
}