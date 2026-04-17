using Laminar.Contracts.Storage.PersistentData;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.Logging;

namespace Laminar.Implementation.Storage.PersistentData;

public class PersistentDataStore : IPersistentDataStore 
{
    public PersistentDataStore(ISerializer serializer, 
        IPersistentDataTranscoder persistentDataTranscoder, 
        ILogger<PersistentDataValue> valueLogger)
    {
        Transcoder = persistentDataTranscoder;
        Root = new PersistentDataNode(serializer, valueLogger)
        {
            Owner = this,
        };
    }

    public IPersistentDataTranscoder? Transcoder { get; }

    public void OnChildChanged()
    {
    }

    public IPersistentDataNode Root { get; }
}