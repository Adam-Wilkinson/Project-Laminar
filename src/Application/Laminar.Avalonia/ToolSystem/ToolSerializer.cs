using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Reactive;
using Laminar.Avalonia.InitializationTargets;
using Laminar.Contracts.UserData;
using Laminar.Domain.DataManagement;
using Laminar.PluginFramework.Serialization;

namespace Laminar.Avalonia.ToolSystem;

public class ToolSerializer(TopLevel topLevel, IPersistentDataManager persistentDataManager, ISerializer serializer) : IAfterApplicationBuiltTarget
{    
    private readonly IPersistentDataStore _toolDataStore = persistentDataManager.GetDataStore(DataStoreKey.ToolProperties);
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
        var uniqueToolKey = $"{prefix}.{tool.Name}";
        
        foreach (var childTool in tool.ChildTools)
        {
            SerializeTool(childTool, uniqueToolKey);
        }

        var currentDataStore = _toolDataStore.CreateChild(uniqueToolKey);
        currentDataStore.InitializeDefaultValue("Key Gesture", tool.Gesture);
        tool.PropertyChanged += (_, _) =>
        {
            currentDataStore.SetItem("Key Gesture", tool.Gesture);
        };

        currentDataStore.GetObservable("Key Gesture").ValueChanged += (_, _) =>
        {
            tool.Gesture = currentDataStore.GetItem<KeyGesture?>("Key Gesture").Result;
        };
    }
}