using System;
using Laminar.Contracts.Storage.PersistentData;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Storage.PersistentData;

public abstract class PersistentDataNode(IServiceProvider serviceProvider) : IPersistentDataValueOwner
{
    public IPersistentDataTranscoder? Transcoder => Owner?.Transcoder;

    public event EventHandler? ChildValueChanged;
    
    public event EventHandler? TranscoderChanged;

    public IPersistentDataValueOwner? Owner
    {
        get;
        set
        {
            if (field is not null)
            {
                field.TranscoderChanged -= OnOwnerTranscoderChanged;
            }
            
            field = value;

            if (field is not null)
            {
                field.TranscoderChanged += OnOwnerTranscoderChanged;
            }
            
            TranscoderChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    
    public void OnChildValueChanged()
    {
        ChildValueChanged?.Invoke(this, EventArgs.Empty);
        Owner?.OnChildValueChanged();
    }

    protected IPersistentDataPoint CreateValue() =>
        ActivatorUtilities.CreateInstance<PersistentDataPoint>(serviceProvider, this);

    protected void RemoveValue(IPersistentDataPoint point)
    {
        point.OnDeletion();
    }
    
    private void OnOwnerTranscoderChanged(object? sender, EventArgs e)
    {
        TranscoderChanged?.Invoke(this, EventArgs.Empty);
    }

}