using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Implementation.Storage.PersistentData;

internal interface IPersistentDataNode : IPersistentDataValueOwner
{
    public IPersistentDataValueOwner? Owner { get; set; }
    
    public bool ChildIsInitializing { get; set; }
    
    public void RemoveChildNode(IPersistentDataNode childNode);
    
    public void RegisterChildNode(IPersistentDataNode childNode);
}