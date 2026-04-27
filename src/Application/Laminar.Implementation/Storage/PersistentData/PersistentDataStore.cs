using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Laminar.Contracts.Base;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Storage.PersistentData;

public class PersistentDataStore : IPersistentDataStore
{
    private static readonly TimeSpan FlushDelay = TimeSpan.FromMilliseconds(200);
    
    private readonly IFileContents _file;
    private readonly ISerializer _serializer;
    private readonly bool _isInitialized = false;
    private readonly IDispatcher _dispatcher;
    
    private CancellationTokenSource? _flushCts; 
    
    public PersistentDataStore(
        IPersistentDataTranscoder persistentDataTranscoder,
        IFileContents file,
        IServiceProvider serviceProvider,
        ISerializer serializer,
        IDispatcher dispatcher)
    {
        _serializer = serializer;
        _file = file;
        _dispatcher = dispatcher;
        Transcoder = persistentDataTranscoder;
        Root = ActivatorUtilities.CreateInstance<PersistentDictionary>(serviceProvider);
        ((PersistentDictionary)Root).Owner = this;
        _isInitialized = true;
        FileToDataNode();
    }

    public IPersistentDataTranscoder Transcoder { get; }
    public event EventHandler? TranscoderChanged { add { } remove { } }

    public void OnChildValueInvalidated()
    {
        if (!_isInitialized) return;
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
                var serialized = _serializer.SerializeObject(Root, typeof(IPersistentDictionary));
                _file.Contents = Transcoder.ToBytes(serialized);
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                await _dispatcher.InvokeAsync(() => ExceptionDispatchInfo.Capture(ex).Throw());
            }
        }, token);
    }
    
    private void FileToDataNode()
    {
        if (_file.Contents.Length == 0)
        {
            return;
        }
        
        var decoded = Transcoder.FromBytes<Dictionary<string, object>>(_file.Contents);
        ArgumentNullException.ThrowIfNull(decoded);
        _serializer.DeserializeObject(new DeserializationRequest
        {
            Serialized = decoded,
            TargetType = typeof(IPersistentDictionary),
            ExistingInstance = Root
        });
    }
}