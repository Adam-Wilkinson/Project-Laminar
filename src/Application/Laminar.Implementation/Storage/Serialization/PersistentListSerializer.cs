using System;
using System.Collections.Generic;
using System.Linq;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Exceptions;
using Laminar.Implementation.Storage.PersistentData;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Storage.Serialization;

public class PersistentListSerializer(IServiceProvider serviceProvider) : 
    TypeSerializer<IPersistentList, List<object>>, INotifyingConditionalSerializer
{
    protected override List<object> SerializeTyped(IPersistentList toSerialize)
        => ((PersistentList)toSerialize).InternalValues.Select(x => x.EncodedValue).ToList();

    protected override IPersistentList DeSerializeTyped(DeserializationRequest<IPersistentList, List<object>> request)
    {
        PersistentList returnValue = request.HasExistingValue
            ? request.ExistingValue as PersistentList ??
              throw new DeserializationError(new ArgumentException("Deserialization requires PersistentList",
                  nameof(request)))
            : ActivatorUtilities.CreateInstance<PersistentList>(serviceProvider);
        
        returnValue.InternalValues.Clear();
        foreach (var encoded in request.Serialized)
        {
            var newValue = returnValue.CreateValue();
            newValue.EncodedValue = encoded;
            returnValue.InternalValues.Add(newValue);
        }
        returnValue.OnContentsChanged();
        
        return returnValue;
    }

    public INotifySerializedValueChanged GetSerializedValueChangedNotifier(object target)
    {
        if (target is not PersistentList persistentList) throw new InvalidCastException();
        return new PersistentListChangedNotifier(persistentList);
    }

    private class PersistentListChangedNotifier : INotifySerializedValueChanged
    {
        private readonly PersistentList _node;
        
        public PersistentListChangedNotifier(PersistentList persistentList)
        {
            _node = persistentList;
            _node.ChildValueChanged += OnSerializedValueChanged;
            _node.ContentsChanged += OnSerializedValueChanged;
        }
        
        public event EventHandler? SerializedValueChanged;

        private void OnSerializedValueChanged(object? sender, EventArgs e)
        {
            SerializedValueChanged?.Invoke(sender, e);
        }
        
        public void Dispose()
        {
            _node.ChildValueChanged -= OnSerializedValueChanged;
            _node.ContentsChanged -= OnSerializedValueChanged;
        }
    }
}