using System.Linq;
using Avalonia.Controls;
using Avalonia.Reactive;
using Laminar.Avalonia.InitializationTargets;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.DataManagement;

namespace Laminar.Avalonia.ToolSystem;

public class ToolSerializer(TopLevel topLevel, IPersistentDataManager persistentDataManager) : IAfterApplicationBuiltTarget
{    
    private const string KeyGesture = "Key Gesture";
    
    private readonly IPersistentDictionary _toolDataStore = persistentDataManager.GetDataStore(DataStoreKey.ToolProperties);
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
        
        var currentDataStore = _toolDataStore[uniqueToolKey]
            .SetDefaultAndGet(persistentDataManager.GetHeadlessNode<IPersistentDictionary>()).Value;
        var persistentGestureValue = currentDataStore[KeyGesture].SetDefaultAndGet(tool.Gesture);
        tool.Gesture = persistentGestureValue.Value;
        tool.PropertyChanged += (_, _) =>
        {
            currentDataStore.SetValue(KeyGesture, tool.Gesture);
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