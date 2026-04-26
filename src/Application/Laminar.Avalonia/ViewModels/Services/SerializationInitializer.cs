using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.DataManagement;

namespace Laminar.Avalonia.ViewModels.Services;

public class SerializationInitializer(IPersistentDataManager dataManager) : IViewModelInitializer
{
    private readonly Dictionary<ViewModelBase, string> _serializationPrefixes = [];
    private readonly Dictionary<Type, Dictionary<string, ISerializedPropertyInfo>> _serializedPropertyInfos = [];
    private readonly IPersistentDictionary _dataStore = dataManager
        .GetDataStore(DataStoreKey.PersistentData)["UserInterface"]
        .SetDefaultAndGet(dataManager.GetHeadlessNode<IPersistentDictionary>()).Value;
    
    public void Initialize(ViewModelBase? parentViewModel, ViewModelBase viewModel, string viewModelName)
    {
        var prefix = (parentViewModel is null ? "" : _serializationPrefixes[parentViewModel]) + "." + viewModelName;
        _serializationPrefixes[viewModel] = prefix;
        var serializedPropertyInfos = GetSerializedPropertyInfos(viewModel.GetType(), viewModel);
        foreach (var property in serializedPropertyInfos.Values)
        {
            property.InitializeToDataStore(prefix, viewModel, _dataStore, property);
        }
        
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is null || !serializedPropertyInfos.TryGetValue(e.PropertyName, out var propertyInfo)) return;
            propertyInfo.PropertyToDataStore(prefix, viewModel, _dataStore);
        };
    }
    
    private Dictionary<string, ISerializedPropertyInfo> GetSerializedPropertyInfos(Type type, object defaultInstance)
    {
        if (_serializedPropertyInfos.TryGetValue(type, out var serializedPropertyInfos))
        {
            return serializedPropertyInfos;
        }

        HashSet<string> fieldPropertyNames = [];

        foreach (var fieldInfo in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                     .Where(field => field.CustomAttributes.Any(x => x.AttributeType == typeof(PersistentAttribute)) 
                                     && field.CustomAttributes.Any(x => x.AttributeType == typeof(ObservablePropertyAttribute))))
        {
            fieldPropertyNames.Add(PropertyName(fieldInfo.Name));
        }

        var retVal = type.GetProperties()
            .Where(property =>
                (property.CustomAttributes.Any(x => x.AttributeType == typeof(PersistentAttribute)) || fieldPropertyNames.Contains(property.Name))
                && property.SetMethod is not null && property.GetMethod is not null)
            .Select(propertyInfo => ISerializedPropertyInfo.Create(propertyInfo, defaultInstance))
            .ToDictionary(x => x.PropertyName);

        _serializedPropertyInfos.Add(type, retVal);
        return retVal;
    }
    
    private static string PropertyName(string fieldName)
    {
        if (fieldName[0] is '_')
        {
            return fieldName[1].ToString().ToUpper() + fieldName[2..];
        }

        return fieldName[0].ToString().ToUpper() + fieldName[1..];
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class PersistentAttribute : Attribute
{
}

public interface ISerializedPropertyInfo
{
    public static ISerializedPropertyInfo Create(PropertyInfo propertyInfo, object defaultInstance)
    {
        return (ISerializedPropertyInfo)Activator.CreateInstance(
            typeof(SerializedPropertyInfo<,>).MakeGenericType(propertyInfo.DeclaringType!, propertyInfo.PropertyType), propertyInfo, defaultInstance)!;
    }

    public string PropertyName { get; }

    public string ValueKey(string prefix);
    
    public void InitializeToDataStore(string prefix, ViewModelBase target, IPersistentDictionary dataStore, ISerializedPropertyInfo externalThis);
    
    public void PropertyToDataStore(string prefix, ViewModelBase target, IPersistentDictionary dataStore);

    private readonly record struct SerializedPropertyInfo<TTarget, TValue>(
        string PropertyName,
        TValue DefaultValue,
        Action<TTarget, TValue> Setter,
        Func<TTarget, TValue> Getter) : ISerializedPropertyInfo where TValue : notnull
    {
        public SerializedPropertyInfo(PropertyInfo pi, object defaultInstance) : this(
            pi.Name,
            (TValue)pi.GetMethod!.Invoke(defaultInstance, [])!,
            ConstructSetter(pi),
            ConstructGetter(pi))
        {
        }

        public string ValueKey(string prefix) => prefix + "." + PropertyName;
        
        public void InitializeToDataStore(string prefix, ViewModelBase target, IPersistentDictionary dataStore, ISerializedPropertyInfo externalThis)
        {
            if (target is not TTarget typedTarget || externalThis is not SerializedPropertyInfo<TTarget, TValue> typedThis)
                throw new ArgumentException("Target is not of type " + typeof(TTarget).FullName);

            var newValue = dataStore[ValueKey(prefix)].SetDefaultAndGet(DefaultValue);
            newValue.OnChanged += (_, e) => typedThis.Setter(typedTarget, e.NewValue);
            Setter(typedTarget, newValue.Value);
        }

        public void PropertyToDataStore(string prefix, ViewModelBase target, IPersistentDictionary dataStore)
        {
            if (target is not TTarget typedTarget)
                throw new ArgumentException("Target is not of type " + typeof(TTarget).FullName);
            
            dataStore.SetValue(ValueKey(prefix), Getter(typedTarget));
        }
        
        private static Func<TTarget, TValue> ConstructGetter(PropertyInfo propertyInfo)
        {
            var parameter = Expression.Parameter(typeof(TTarget));

            Expression accessPropertyValue = Expression.Property(parameter, propertyInfo);
            if (propertyInfo.PropertyType != typeof(TValue)) accessPropertyValue = Expression.Convert(accessPropertyValue, typeof(TValue));

            return Expression.Lambda<Func<TTarget, TValue>>(accessPropertyValue, parameter).Compile();
        }
        
        private static Action<TTarget, TValue> ConstructSetter(PropertyInfo propertyInfo)
        {
            var instanceParameter = Expression.Parameter(typeof(TTarget));
            var valueParameter = Expression.Parameter(typeof(TValue));

            Expression propertyExpression = Expression.Property(instanceParameter, propertyInfo);
            Expression valueExpression = valueParameter;
            if (propertyInfo.PropertyType != typeof(TValue)) valueExpression = Expression.Convert(valueExpression, propertyInfo.PropertyType);

            var assignExpression = Expression.Assign(propertyExpression, valueExpression);

            return Expression.Lambda<Action<TTarget, TValue>>(assignExpression, instanceParameter, valueParameter).Compile();
        }
    }
}