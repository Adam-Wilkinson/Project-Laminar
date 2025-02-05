using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Laminar.Avalonia.InitializationTargets;
using Laminar.Contracts.UserData;
using Laminar.Domain.DataManagement;

namespace Laminar.Avalonia.ViewModels.Services;

public class ViewModelSerializationHelper(TopLevel topLevel, IPersistentDataManager dataManager) : IAfterApplicationBuiltTarget
{
    private readonly HashSet<ViewModelBase> _registeredViewModels = [];
    private readonly Dictionary<Type, Dictionary<string, ISerializedPropertyInfo>> _serializedPropertyInfos = [];
    private readonly IPersistentDataStore _dataStore = dataManager.GetDataStore(DataStoreKey.PersistentData);
    
    public void OnApplicationBuilt()
    {
        if (topLevel.DataContext is ViewModelBase topLevelViewModel)
        {
            SerializeObservableProperties(topLevelViewModel, _dataStore);
        }
    }
    
    public void SerializeObservableProperties(ViewModelBase viewModel, IPersistentDataStore dataStore, string prefix = "")
    {
        _registeredViewModels.Add(viewModel);
        foreach (var propertyInfo in viewModel.GetType().GetProperties())
        {
            if (propertyInfo.GetMethod is not null 
                && propertyInfo.PropertyType.IsSubclassOf(typeof(ViewModelBase))
                && propertyInfo.GetMethod.Invoke(viewModel, []) is ViewModelBase childViewModel
                && !_registeredViewModels.Contains(childViewModel))
            {
                SerializeObservableProperties(childViewModel, dataStore, string.IsNullOrEmpty(prefix) ? propertyInfo.Name : (prefix + "." + propertyInfo.Name));
            }
        }
        
        var serializedPropertyInfos = GetSerializedPropertyInfos(viewModel.GetType(), viewModel);
        foreach (var property in serializedPropertyInfos.Values)
        {
            property.InitializeToDataStore(prefix, viewModel, dataStore);
            dataStore.GetObservable(property.ValueKey(prefix)).ValueChanged += (_, _) =>
            {
                property.DataStoreToProperty(prefix, viewModel, dataStore);
            };
        }
        
        foreach (var property in serializedPropertyInfos.Values)
        {
            property.DataStoreToProperty(prefix, viewModel, dataStore);
        }
        
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is null || !serializedPropertyInfos.TryGetValue(e.PropertyName, out var propertyInfo)) return;
            propertyInfo.PropertyToDataStore(prefix, viewModel, dataStore);
        };
    }
    
    private Dictionary<string, ISerializedPropertyInfo> GetSerializedPropertyInfos(Type type, object defaultInstance)
    {
        if (_serializedPropertyInfos.TryGetValue(type, out var serializedPropertyInfos))
        {
            return serializedPropertyInfos;
        }

        HashSet<string> fieldPropertyNames = [];
        List<ISerializedPropertyInfo> serializedPropertyList = [];

        foreach (var fieldInfo in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                     .Where(field => field.CustomAttributes.Any(x => x.AttributeType == typeof(SerializeAttribute)) 
                                     && field.CustomAttributes.Any(x => x.AttributeType == typeof(ObservablePropertyAttribute))))
        {
            fieldPropertyNames.Add(PropertyName(fieldInfo.Name));
        }

        serializedPropertyList.AddRange(type.GetProperties()
            .Where(property =>
                (property.CustomAttributes.Any(x => x.AttributeType == typeof(SerializeAttribute)) ||
                 fieldPropertyNames.Contains(property.Name))
                && property.SetMethod is not null && property.GetMethod is not null)
            .Select(propertyInfo => ISerializedPropertyInfo.Create(propertyInfo, defaultInstance)));

        var retVal = serializedPropertyList.ToDictionary(x => x.PropertyName);
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
public class SerializeAttribute : Attribute
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
    
    public void InitializeToDataStore(string prefix, ViewModelBase deserializationContext, IPersistentDataStore dataStore);

    public void DataStoreToProperty(string prefix, ViewModelBase target, IPersistentDataStore dataStore);
    
    public void PropertyToDataStore(string prefix, ViewModelBase target, IPersistentDataStore dataStore);

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
        
        public void InitializeToDataStore(string prefix, ViewModelBase deserializationContext, IPersistentDataStore dataStore)
        {
            dataStore.InitializeDefaultValue(ValueKey(prefix), DefaultValue, deserializationContext);
        }

        public void DataStoreToProperty(string prefix, ViewModelBase target, IPersistentDataStore dataStore)
        {
            if (target is not TTarget typedTarget)
                throw new ArgumentException("Target is not of type " + typeof(TTarget).FullName);
            
            var readResult = dataStore.GetItem<TValue>(ValueKey(prefix)); 
            if (readResult.Result is not { } dataStoreValue)
            {
                throw new Exception("Error reading value for " + PropertyName + " with status " + readResult.Status, readResult.Exception);
            }

            Setter(typedTarget, dataStoreValue);
        }

        public void PropertyToDataStore(string prefix, ViewModelBase target, IPersistentDataStore dataStore)
        {
            if (target is not TTarget typedTarget)
                throw new ArgumentException("Target is not of type " + typeof(TTarget).FullName);
            
            dataStore.SetItem(ValueKey(prefix), Getter(typedTarget));
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