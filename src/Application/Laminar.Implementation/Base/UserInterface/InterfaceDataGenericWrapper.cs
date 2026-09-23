using System.ComponentModel;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Base.UserInterface;

public class InterfaceDataGenericWrapper<TInterfaceDefinition, TValue> : IInterfaceData<TInterfaceDefinition, TValue>, IDisposable
    where TInterfaceDefinition : IUserInterfaceDefinition where TValue : notnull
{
    private readonly IInterfaceData _internal;
    private readonly IInterfaceData<TValue>? _genericDataInternal;
    
    public InterfaceDataGenericWrapper(IInterfaceData<TValue> interfaceData, TInterfaceDefinition interfaceDefinition) : this((IInterfaceData)interfaceData, interfaceDefinition)
    {
        _genericDataInternal = interfaceData;
    }

    public InterfaceDataGenericWrapper(IInterfaceData interfaceData, TInterfaceDefinition interfaceDefinition)
    {
        _internal = interfaceData;
        _internal.PropertyChanged += InterfaceData_PropertyChanged;
        Definition = interfaceDefinition;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    
    public bool IsUserEditable => _internal.IsUserEditable;

    public TValue Value
    {
        get => _genericDataInternal is not null ? _genericDataInternal.Value : (TValue)_internal.Value;
        set
        {
            if (!IsUserEditable) throw new InvalidOperationException();
            if (_genericDataInternal is not null)
            {
                _genericDataInternal.Value = value;
            }
            else
            {
                _internal.Value = value;
            }
        }
    }

    public string Name => _internal.Name;

    private void InterfaceData_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IInterfaceData.Definition)) return;
        PropertyChanged?.Invoke(this, e);
    }

    public TInterfaceDefinition Definition { get; }
    
    public void SetValue(TValue value)
    {
        if (_genericDataInternal is not null)
        {
            _genericDataInternal.SetValue(value);
        }
        else
        {
            _internal.SetValue(value);
        }
    }

    public void Dispose()
    {
        _internal.PropertyChanged -= InterfaceData_PropertyChanged;
        GC.SuppressFinalize(this);
    }
}