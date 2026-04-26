using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Laminar.Contracts.Base;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.PersistentData;

public class PersistentDataStore : IPersistentDataStore
{
    private readonly IFileContents _file;
    private readonly ISerializer _serializer;
    private readonly bool _isInitialized = false;
    
    public PersistentDataStore(
        IServiceProvider serviceProvider,
        IPersistentDataTranscoder persistentDataTranscoder,
        IFileContents file,
        ISerializer serializer)
    {
        _serializer = serializer;
        _file = file;
        Transcoder = persistentDataTranscoder;
        Root = ActivatorUtilities.CreateInstance<PersistentDictionary>(serviceProvider);
        ((PersistentDictionary)Root).Owner = this;
        _isInitialized = true;
        FileToDataNode();
        // _file.ContentsChanged += (_, _) => FileToDataNode();
    }

    public IPersistentDataTranscoder Transcoder { get; }
    public event EventHandler? TranscoderChanged { add { } remove { } }

    public void OnChildValueChanged()
    {
        if (!_isInitialized) return;
        var serialized = _serializer.SerializeObject(Root, typeof(IPersistentDictionary));
        _file.Contents = Transcoder.ToBytes(serialized);
    }

    public IPersistentDictionary Root { get; }

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