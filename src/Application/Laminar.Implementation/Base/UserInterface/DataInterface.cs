using System.ComponentModel;
using Laminar.Contracts.Base.UserInterface;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.Implementation.Base.UserInterface;

public class DataInterface<TFrontend> : IDataInterface<TFrontend>, IRefreshable
    where TFrontend : class, new()
{
    private readonly IInterfaceData _interfaceData;
    private readonly DataInterfaceFactory _dataInterfaceFactory;
    private readonly bool _valueTypeIsMutable;
    
    private Type _valueType;
    
    public DataInterface(IInterfaceData interfaceData, DataInterfaceFactory dataInterfaceFactory)
    {
        _interfaceData = interfaceData;
        _valueType = _interfaceData.Value.GetType();
        _valueTypeIsMutable = !(interfaceData.GetType().IsGenericType && interfaceData.GetType().GetGenericTypeDefinition() == typeof(IInterfaceData<>));
        _dataInterfaceFactory = dataInterfaceFactory;
        interfaceData.PropertyChanged += OnInterfaceDataPropertyChanged;

        (InterfaceFrontend, InterfaceData) = _dataInterfaceFactory.GetFrontendAndData<TFrontend>(_interfaceData);
    }

    private void OnInterfaceDataPropertyChanged(object? _, PropertyChangedEventArgs args)
    {
        if (args.PropertyName is nameof(IInterfaceData.Definition) or nameof(IInterfaceData.IsUserEditable))
        {
            Refresh();
        }

        if (args.PropertyName == nameof(IInterfaceData.Value) && _valueTypeIsMutable && _interfaceData.Value.GetType() != _valueType)
        {
            _valueType = _interfaceData.Value.GetType();
            Refresh();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    
    public IInterfaceData? InterfaceData { get; private set; }
    
    public TFrontend InterfaceFrontend { get; private set; }
    
    public void Refresh()
    {
        (InterfaceData as IDisposable)?.Dispose();
        InterfaceData = null;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InterfaceData)));
        
        (InterfaceFrontend, InterfaceData) = _dataInterfaceFactory.GetFrontendAndData<TFrontend>(_interfaceData);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InterfaceFrontend)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InterfaceData)));
    }
}
