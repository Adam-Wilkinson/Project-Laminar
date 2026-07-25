using System.ComponentModel;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.PluginFramework.UserInterface;

public interface IInterfaceData<out TInterfaceDefinition, TValue> : IInterfaceData<TValue>
    where TInterfaceDefinition : IUserInterfaceDefinition where TValue : notnull
{
    public new TInterfaceDefinition Definition { get; }

    IUserInterfaceDefinition IInterfaceData.Definition => Definition;
}

public interface IInterfaceData<TValue> : IInterfaceData where TValue : notnull
{
    public new TValue Value { get; set; }

    /// <summary>
    /// Sets the value.
    /// Unlike the setter of <see cref="Value"/>, this also works when <see cref="IInterfaceData.IsUserEditable"/> is false, and should be treated as quiet access to the internal state of the interface data.    
    /// </summary>
    /// <param name="newValue">The new value</param>
    public void SetValue(TValue newValue);
    
    object IInterfaceData.Value
    {
        get => Value;
        set => Value = (TValue)value;
    }
    
    void IInterfaceData.SetValue(object newValue) => SetValue((TValue)newValue); 
}

public interface IInterfaceData : INotifyPropertyChanged
{
    public string Name { get; }

    public object Value { get; set; }

    public bool IsUserEditable { get; }
    
    public IUserInterfaceDefinition? Definition { get; }

    /// <summary>
    /// Sets the value.
    /// Unlike the setter of <see cref="Value"/>, this also works when <see cref="IsUserEditable"/> is false, and should be treated as quiet access to the internal state of the interface data.    
    /// </summary>
    /// <param name="newValue">The new value</param>
    public void SetValue(object newValue);
}