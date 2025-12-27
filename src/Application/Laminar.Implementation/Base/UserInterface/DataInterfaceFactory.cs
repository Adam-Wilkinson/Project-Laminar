using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.UserInterface;
using Laminar.PluginFramework;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Base.UserInterface;

public class DataInterfaceFactory(ITypeInfoStore typeInfoStore) : IDataInterfaceFactory
{
    private delegate IInterfaceData? GenericDataFactory(IInterfaceData inputData, IUserInterfaceDefinition userInterfaceDefinition);
    
    private readonly Dictionary<Type, List<(Type valueType, GenericDataFactory factory)>> _interfaceFactories = [];
    private readonly Dictionary<Type, List<Type>> _frontendImplementations = [];
    
    public void RegisterInterface<TInterfaceDefinition, TValue, TInterface>()
        where TInterfaceDefinition : IUserInterfaceDefinition, new()
        where TValue : notnull
    {
        if (!_interfaceFactories.ContainsKey(typeof(TInterfaceDefinition)))
        {
            _interfaceFactories.Add(typeof(TInterfaceDefinition), []);
        }
        
        _interfaceFactories[typeof(TInterfaceDefinition)].Add((typeof(TValue), CreateGenericDataFactory<TInterfaceDefinition, TValue>()));
        if (!_frontendImplementations.ContainsKey(typeof(TInterfaceDefinition)))
        {
            _frontendImplementations.Add(typeof(TInterfaceDefinition), []);
        }
        
        _frontendImplementations[typeof(TInterfaceDefinition)].Add(typeof(TInterface));
    }

    public IDataInterface<TFrontend> GetDataInterface<TFrontend>(IInterfaceData interfaceData)
        where TFrontend : class, new() => new DataInterface<TFrontend>(interfaceData, this);

    public (TFrontend, IInterfaceData) GetFrontendAndData<TFrontend>(IInterfaceData interfaceData)
        where TFrontend : class, new()
    {
        if (interfaceData.Definition is not null 
            && TryGetFrontendAndData<TFrontend>(interfaceData, interfaceData.Definition) is { } idealResult)
        {
            return idealResult;
        }

        if (typeInfoStore.TryGetTypeInfo(interfaceData.Value.GetType(), out var interfaceDataTypeInfo))
        {
            var requestedDefinition = interfaceData.IsUserEditable ? interfaceDataTypeInfo.EditorDefinition : interfaceDataTypeInfo.ViewerDefinition;
            if (requestedDefinition is IUserInterfaceDefinition requestedInterfaceDefinition 
                && TryGetFrontendAndData<TFrontend>(interfaceData, requestedInterfaceDefinition) is { } typeDefaultResult)
            {
                return typeDefaultResult;
            }
        }

        if (interfaceData.Value.GetType().IsEnum && TryGetFrontendAndData<TFrontend>(interfaceData, new EnumDropdown()) is { } enumResult)
        {
            return enumResult;
        }

        var defaultViewerData = new InterfaceDataGenericWrapper<DefaultViewer, None>(interfaceData, new DefaultViewer());
        if (GetFrontendFromData<TFrontend>(defaultViewerData) is { } defaultResult)
        {
            return (defaultResult, interfaceData);
        }

        throw new Exception($"No default viewer found for frontend of type {typeof(TFrontend)}");
    }

    private (TFrontend frontend, IInterfaceData genericData)? TryGetFrontendAndData<TFrontend>(IInterfaceData inputData, IUserInterfaceDefinition interfaceDefinition) 
        where TFrontend : class, new()
    {
        if (!_interfaceFactories.TryGetValue(interfaceDefinition.GetType(), out var genericInterfaceDataFactories))
        {
            return null;
        }

        foreach (var genericInterfaceDataFactory in genericInterfaceDataFactories)
        {
            if (ValueTypeIsCompatible(inputData, genericInterfaceDataFactory.valueType)
                && genericInterfaceDataFactory.factory(inputData, interfaceDefinition) is { } genericInterfaceData
                && GetFrontendFromData<TFrontend>(genericInterfaceData) is { } frontend)
            {
                return (frontend, genericInterfaceData);
            }
        }

        return null;
    }

    private static bool ValueTypeIsCompatible(IInterfaceData interfaceData, Type valueType)
    {
        if (interfaceData.GetType().GetInterfaces().Any(x =>
                x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IInterfaceData<,>) &&
                x.GetGenericArguments()[1] == valueType))
        {
            return true;
        }
        
        return interfaceData.IsUserEditable switch
        {
            true => interfaceData.Value.GetType() == valueType,
            false => valueType.IsInstanceOfType(interfaceData.Value)
        };
    }

    private TFrontend? GetFrontendFromData<TFrontend>(IInterfaceData interfaceData) 
        where TFrontend : class, new()
    {
        if (interfaceData.Definition is null || !_frontendImplementations.TryGetValue(interfaceData.Definition.GetType(), out var frontendTypes))
        {
            return null;
        }
        
        foreach (var frontendType in frontendTypes)
        {
            if (typeof(TFrontend).IsAssignableFrom(frontendType) && Activator.CreateInstance(frontendType) is TFrontend frontend)
            {
                return frontend;
            }
        }

        return null;
    }
    
    private static GenericDataFactory CreateGenericDataFactory<TInterfaceDefinition, TValue>() 
        where TInterfaceDefinition : IUserInterfaceDefinition, new() 
        where TValue : notnull
        => (interfaceData, interfaceDefinition) => interfaceData switch {
            IInterfaceData<TInterfaceDefinition, TValue> genericInterfaceData => genericInterfaceData,
            IInterfaceData<TValue> genericValueInterfaceData => new InterfaceDataGenericWrapper<TInterfaceDefinition, TValue>(genericValueInterfaceData, (TInterfaceDefinition)interfaceDefinition),
            not null => new InterfaceDataGenericWrapper<TInterfaceDefinition, TValue>(interfaceData, (TInterfaceDefinition)interfaceDefinition),
            _ => null,
        };
}

public class InterfaceDataGenericWrapper<TInterfaceDefinition, TValue> : IInterfaceData<TInterfaceDefinition, TValue>
    where TInterfaceDefinition : IUserInterfaceDefinition, new() where TValue : notnull
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
            if (_genericDataInternal is not null) _genericDataInternal.Value = value;
            else _internal.Value = value;
        }
    }

    public string Name => _internal.Name;

    private void InterfaceData_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IInterfaceData.Definition)) return;
        PropertyChanged?.Invoke(this, e);
    }

    public TInterfaceDefinition Definition { get; }
}