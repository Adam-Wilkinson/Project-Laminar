using System;
using System.Collections.Generic;
using Laminar.Contracts.Storage.PersistentData;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Storage.PersistentData;

public abstract class PersistentDataNode(IServiceProvider serviceProvider) : IPersistentDataValueOwner
{
    private readonly HashSet<PersistentDataNode> _children = [];
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
    
    public void OnChildValueChanged()
    {
        if (_transcoderChanging || ChildIsInitializing) return;
        ChildValueChanged?.Invoke(this, EventArgs.Empty);
        Owner?.OnChildValueChanged();
    }

    public IPersistentDataPoint CreateValue() =>
        ActivatorUtilities.CreateInstance<PersistentDataPoint>(serviceProvider, this);

    public void RegisterChildNode(PersistentDataNode child)
    {
        _children.Add(child);
        child.Owner = this;
    }

    public void RemoveChildNode(PersistentDataNode child) => _children.Remove(child);
    
    protected abstract void BeforeTranscoderChangedEvent();
    
    protected void RemoveValue(IPersistentDataPoint point)
    {
        point.OnDeletion();
    }

    private void OnTranscoderChanged()
    {
        _transcoderChanging = true;
        
        foreach (var child in _children)
        {
            child.OnTranscoderChanged();
        }
        
        TranscoderChanged?.Invoke(this, EventArgs.Empty);
        
        _transcoderChanging = false;
        OnChildValueChanged();
    }
}