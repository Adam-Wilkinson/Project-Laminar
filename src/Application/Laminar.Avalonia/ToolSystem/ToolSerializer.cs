using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Reactive;
using Laminar.Avalonia.InitializationTargets;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.DataManagement;
using Laminar.Domain.Extensions;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Avalonia.ToolSystem;

public class ToolSerializer(TopLevel topLevel, IPersistentDataManager persistentDataManager) : IAfterApplicationBuiltTarget
{    
    private readonly IPersistentDataNode _toolDataStore = persistentDataManager.GetDataStore(DataStoreKey.ToolProperties);
    private bool _initialized;
    
    public void OnApplicationBuilt()
    {
        topLevel.GetResourceObservable(Tool.ToolRootKey).Subscribe(new AnonymousObserver<object?>(x =>
        {
            if (x is Tool tool && !_initialized)
            {
                _initialized = true;
                SerializeTool(tool, "");
            }
        }));
    }

    private void SerializeTool(Tool tool, string prefix)
    {
        var uniqueToolKey = $"{prefix}.{tool.NameKey}";

        foreach (var childTool in tool.ChildTools ?? Enumerable.Empty<Tool>())
        {
            SerializeTool(childTool, uniqueToolKey);
        }

        var currentDataStore = _toolDataStore.GetOrCreateChild(uniqueToolKey);
        var persistentGestureValue = currentDataStore.InitializeDefaultValue("Key Gesture", tool.Gesture);
        tool.Gesture = persistentGestureValue.Value;
        tool.PropertyChanged += (_, _) =>
        {
            currentDataStore.SetValue("Key Gesture", tool.Gesture);
        };

        persistentGestureValue.OnChanged += (_, e) =>
        {
            tool.Gesture = e.NewValue;
        };
        
        tool.ResetKeybindingRequested += (_, _) =>
        {
            persistentGestureValue.Reset();
        };
    }
}