using Laminar.Contracts.Storage.PersistentData;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Storage.PersistentData;

internal abstract class PersistentDataNode(IServiceProvider serviceProvider) : IPersistentDataNode
{
    private readonly HashSet<IPersistentDataNode> _children = [];
    private bool _transcoderChanging;

    public IPersistentDataTranscoder? Transcoder => Owner?.Transcoder;

    public event EventHandler? ChildValueChanged;
    
    public event EventHandler? TranscoderChanged;

    public bool ChildIsInitializing { get; set; }

    public IPersistentDataValueOwner? Owner
    {
        get;
        set
        {
            if (Equals(field, value)) return;
            field = value;
            OnTranscoderChanged();
        }
    }

    public void OnChildValueInvalidated()
    {
        if (_transcoderChanging || ChildIsInitializing) return;
        ChildValueChanged?.Invoke(this, EventArgs.Empty);
        Owner?.OnChildValueInvalidated();
    }

    public IPersistentDataPoint CreateValue() =>
        ActivatorUtilities.CreateInstance<PersistentDataPoint>(serviceProvider, this);

    public void RegisterChildNode(IPersistentDataNode child) => _children.Add(child);

    public void RemoveChildNode(IPersistentDataNode child) => _children.Remove(child);
    
    protected void RemoveValue(IPersistentDataPoint point)
    {
        point.OnDeletion();
    }

    public void OnTranscoderChanged()
    {
        _transcoderChanging = true;
        
        foreach (var child in _children)
        {
            if (child is PersistentDataNode childNode)
            {
                childNode.OnTranscoderChanged();
            }
        }
        
        TranscoderChanged?.Invoke(this, EventArgs.Empty);
        
        _transcoderChanging = false;
        OnChildValueInvalidated();
    }
}