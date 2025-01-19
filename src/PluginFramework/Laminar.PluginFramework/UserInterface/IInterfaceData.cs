using System.ComponentModel;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.PluginFramework.UserInterface;

public interface IInterfaceData<out TInterfaceDefinition, TValue> : IInterfaceData<TValue>
    where TInterfaceDefinition : IUserInterfaceDefinition, new() where TValue : notnull
{
    public new TInterfaceDefinition Definition { get; }

    IUserInterfaceDefinition IInterfaceData.Definition => Definition;
}

public interface IInterfaceData<TValue> : IInterfaceData where TValue : notnull
{
    public new TValue Value { get; set; }

    object IInterfaceData.Value
    {
        get => Value;
        set => Value = (TValue)value;
    }
}

public interface IInterfaceData : INotifyPropertyChanged
{
    public string Name { get; }

    public object Value { get; set; }

    public bool IsUserEditable { get; }
    
    public IUserInterfaceDefinition? Definition { get; }
}