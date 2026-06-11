using System.ComponentModel;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Notification.Value;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Base.UserInterface;

public class ObservableValueInterfaceData<TEditor, TValue>(IObservableValue<TValue> observableValue) 
    : IInterfaceData<TEditor, TValue> where TEditor : IUserInterfaceDefinition where TValue : notnull
{
    public event PropertyChangedEventHandler? PropertyChanged
    {
        add => observableValue.PropertyChanged += value;
        remove => observableValue.PropertyChanged -= value;
    }
    
    public required string Name { get; init; }

    public TValue Value
    {
        get => observableValue.Value;
        set => observableValue.Value = value;
    }
    
    public required TEditor Definition { get; init; }
    
    public bool IsUserEditable => true;
    
    public void SetValue(TValue newValue) => Value = newValue;
}

public static class ObservableValueInterfaceDataExtensions
{
    extension(IPersistentDictionary persistentDictionary)
    {
        public IInterfaceData<TEditor, TValue> GetValueInterface<TEditor, TValue>(string valueName, TEditor definition) 
            where TEditor : IUserInterfaceDefinition where TValue : notnull
        {
            return new ObservableValueInterfaceData<TEditor, TValue>(persistentDictionary[valueName].GetValue<TValue>())
            {
                Name = valueName,
                Definition = definition
            };
        }
        
        public IInterfaceData<TEditor, TValue> GetValueInterface<TEditor, TValue>(TValue defaultValue, string valueName, TEditor definition) 
            where TEditor : IUserInterfaceDefinition where TValue : notnull
        {
            return new ObservableValueInterfaceData<TEditor, TValue>(persistentDictionary[valueName].GetValueOrDefault(defaultValue))
            {
                Name = valueName,
                Definition = definition
            };
        }
    }
}