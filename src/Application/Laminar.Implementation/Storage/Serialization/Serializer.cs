using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Storage.Serialization;

public class Serializer : ISerializer
{
    private readonly IServiceProvider _serviceProvider;

    private readonly HashSet<Assembly> _scannedAssemblies = [];
    private readonly DefaultSerializerFactory _defaultSerializerFactory; 
    private readonly Dictionary<Type, IConditionalSerializer> _typeSerializers = [];
    private readonly List<IConditionalSerializer> _conditionalSerializers = [];
    private readonly List<IConditionalSerializerFactory> _conditionalSerializerFactories = [];

    public Serializer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _defaultSerializerFactory = new DefaultSerializerFactory(this);
    }

    public void RegisterSerializer(IConditionalSerializer serializer)
    {
        switch (serializer)
        {
            case TypeSerializer typeSerializer:
                _typeSerializers[typeSerializer.Type] = typeSerializer;
                break;
            default:
                _conditionalSerializers.Add(serializer);
                break;
        }
    }

    public void RegisterFactory(IConditionalSerializerFactory factory)
    {
        _conditionalSerializerFactories.Add(factory);
    }

    public object SerializeObject(object toSerialize, Type? overrideTypeKey = null)
        => GetSerializer(overrideTypeKey ?? toSerialize.GetType()).Serialize(toSerialize);

    public object DeserializeObject(object serialized, Type requestedType, object? context)
        => GetSerializer(requestedType).DeSerialize(serialized, context);

    public Type GetSerializedType(Type typeToSerialize)
    {
        return GetSerializer(typeToSerialize).SerializedTypeOrNull(typeToSerialize)!;
    }
    
    private IConditionalSerializer GetSerializer(Type typeToSerialize)
    {
        // The Implementation assembly. If we do this in the constructor, the app doesn't start
        EnsureAssemblyInit(typeof(Serializer).Assembly);
        EnsureAssemblyInit(typeToSerialize.Assembly);
        
        if (_typeSerializers.TryGetValue(typeToSerialize, out var typeSerializer))
        {
            return typeSerializer;
        }

        foreach (var factory in _conditionalSerializerFactories)
        {
            if (factory.TryCreateSerializerFor(typeToSerialize) is { } serializer)
            {
                _typeSerializers.Add(typeToSerialize, serializer);
                return serializer;
            }
        }
        
        if (_conditionalSerializers.FirstOrDefault(serializer => serializer.SerializedTypeOrNull(typeToSerialize) is not null) is
            { } conditionalSerializer)
        {
            _typeSerializers.Add(typeToSerialize, conditionalSerializer);
            return conditionalSerializer;
        }

        if (_defaultSerializerFactory.TryCreateSerializerFor(typeToSerialize) is { } defaultSerializer)
        {
            return defaultSerializer;
        }
        
        throw new NotSupportedException($"No serializer found for type {typeToSerialize.FullName}.");
    }

    public void EnsureAssemblyInit(Assembly assembly)
    {
        if (!_scannedAssemblies.Add(assembly))
        {
            return;
        }

        foreach (var type in assembly.GetTypes())
        {
            if (!type.ContainsGenericParameters 
                && type != typeof(PrimitiveSerializer)
                && type.GetInterfaces().Contains(typeof(IConditionalSerializer))
                && ActivatorUtilities.CreateInstance(_serviceProvider, type) is IConditionalSerializer conditionalSerializer)
            {
                RegisterSerializer(conditionalSerializer);
            }

            if (!type.ContainsGenericParameters 
                && type.GetInterfaces().Contains(typeof(IConditionalSerializerFactory))
                && type != typeof(DefaultSerializerFactory)
                && ActivatorUtilities.CreateInstance(_serviceProvider, type) is IConditionalSerializerFactory factory)
            {
                RegisterFactory(factory);
            }
        }
    }
}