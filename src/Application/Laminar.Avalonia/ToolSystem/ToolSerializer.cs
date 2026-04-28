using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Reactive;
using Laminar.Avalonia.InitializationTargets;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.DataManagement;
using Laminar.Domain.ValueObjects;

namespace Laminar.Avalonia.ToolSystem;

public class ToolSerializer(TopLevel topLevel, IPersistentDataManager persistentDataManager) : IAfterApplicationBuiltTarget
{    
    private const string KeyGesture = "Key Gesture";
    private const string QuickAccess = "Quick Access";
    
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
        
        var currentToolDataStore = _toolDataStore[uniqueToolKey]
            .SetDefaultAndGet(persistentDataManager.GetHeadlessNode<IPersistentDictionary>()).Value;
        
        var persistentGestureValue = currentToolDataStore[KeyGesture].SetDefaultAndGet(tool.Gesture);

        tool[!Tool.GestureProperty] = persistentGestureValue.ToBinding();

        tool.ResetKeybindingRequested += (_, _) =>
        {
            persistentGestureValue.Reset();
        };
        
        var persistentQuickAccess = currentToolDataStore[QuickAccess]
            .SetDefaultAndGet(tool.QuickAccess);
        
        tool.QuickAccess.Clear();
        tool.QuickAccess.AddRange(persistentQuickAccess.Value);
        bool quickAccessChanging = false;
        
        tool.QuickAccess.PropertyChanged += (_, _) =>
        {
            if (quickAccessChanging) return;
            quickAccessChanging = true;
            persistentQuickAccess.Value.Clear();
            persistentQuickAccess.Value.AddRange(tool.QuickAccess);
            quickAccessChanging = false;
        };

        persistentQuickAccess.Value.PropertyChanged += PersistentQuickAccessOnPropertyChanged;

        persistentQuickAccess.OnChanged += (_, e) =>
        {
            e.OldValue.PropertyChanged -= PersistentQuickAccessOnPropertyChanged;
            e.NewValue.PropertyChanged += PersistentQuickAccessOnPropertyChanged;
        };
        
        return;
        void PersistentQuickAccessOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (quickAccessChanging) return;
            quickAccessChanging = true;
            tool.QuickAccess.Clear();
            tool.QuickAccess.AddRange(persistentQuickAccess.Value);
            quickAccessChanging = false;
        }
    }
}