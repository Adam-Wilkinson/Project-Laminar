using System;
using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Implementation.Storage.PersistentData;

public interface IPersistentValueInternal
{
    public object GetEncoded(IPersistentDataTranscoder transcoder);

    public bool TrySetFromEncoded(object encodedValue, IPersistentDataTranscoder transcoder);

    public void SetDataOwner(IPersistentDataValueOwner? newOwner);

    public Type TypeSerializationKey { get; }
}