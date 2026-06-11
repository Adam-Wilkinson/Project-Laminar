using System.ComponentModel;
using System.Runtime.CompilerServices;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.PluginFramework.UserInterface;

public sealed class InterfaceData<TInterfaceDefinition, TValue> : IInterfaceData<TInterfaceDefinition, TValue>
    where TInterfaceDefinition : IUserInterfaceDefinition, new()
    where TValue : notnull
{
    private TValue _value = default!;
    
    public TValue Value
    {
        get => _value;
        set
        {
            if (!IsUserEditable) throw new InvalidOperationException("This value is not user editable");
            SetField(ref _value, value);
        }
    }

    public bool IsUserEditable { get; init; } = false;

    public TInterfaceDefinition Definition
    {
        get;
        set => SetField(ref field, value);
    } = new();

    public void SetValue(TValue newValue) => SetField(ref _value, newValue, nameof(Value));

    public required string Name { get; init; }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}