using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Laminar.Contracts.Storage.IO;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.PersistentData;

public class PersistentDataStore : IPersistentDataStore
{
    private readonly IFileContents _file;
    private readonly ISerializer _serializer;
    
    public PersistentDataStore(
        IPersistentDataTranscoder persistentDataTranscoder,
        IFileContents file,
        ISerializer serializer, 
        ILogger<PersistentDataValue> valueLogger)
    {
        _serializer = serializer;
        Transcoder = persistentDataTranscoder;
        Root = new PersistentDataNode(serializer, valueLogger)
        {
            Owner = this,
        };
        _file = file;
        // _file.ContentsChanged += (_, _) => FileToDataNode();
    }

    public IPersistentDataTranscoder Transcoder { get; }
    public event EventHandler? TranscoderChanged { add { } remove { } }

    public void OnChildValueChanged()
    {
        var serialized = _serializer.SerializeObject(Root, typeof(PersistentDataNode));
        _file.Contents = Transcoder.ToBytes(serialized);
    }

    public IPersistentDataNode Root { get; }

    private void FileToDataNode()
    {
        var decoded = Transcoder.FromBytes<Dictionary<string, object>>(_file.Contents);
        ArgumentNullException.ThrowIfNull(decoded);
        _serializer.DeserializeObject(decoded, typeof(PersistentDataNode), Root);
    }
}