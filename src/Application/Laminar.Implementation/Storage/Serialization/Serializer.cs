using System.Reflection;
using Laminar.PluginFramework.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Laminar.Implementation.Storage.Serialization;

public class Serializer(IServiceProvider serviceProvider) : ISerializer
{
    private static readonly INotifySerializedValueChanged DefaultNotifier = new NullNotifier();

    private static readonly PrimitiveSerializer PrimitiveSerializer = new();
    private readonly HashSet<Assembly> _scannedAssemblies = [];
    private readonly List<IConditionalSerializer> _conditionalSerializers = [];
    private readonly List<IConditionalSerializerFactory> _conditionalSerializerFactories = [];
    private readonly Dictionary<Type, IConditionalSerializer> _typeSerializerMap = [];

    public void RegisterSerializer(IConditionalSerializer serializer)
    {
        switch (serializer)
        {
            case TypeSerializer typeSerializer:
                _typeSerializerMap[typeSerializer.Type] = typeSerializer;
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

    public object DeserializeObject(DeserializationRequest request)
        => GetSerializer(request.TargetType).DeSerialize(request);

    public Type GetSerializedType(Type typeToSerialize)
    {   
        return GetSerializer(typeToSerialize).SerializedTypeOrNull(typeToSerialize)!;
    }

    public INotifySerializedValueChanged GetSerializedValueChangedNotifier(object target, Type? overrideTypeKey = null) =>
        GetSerializer(overrideTypeKey ?? target.GetType()) is INotifyingConditionalSerializer notifyingSerializer
            && notifyingSerializer.GetSerializedValueChangedNotifier(target) is { } notifier
            ? notifier : DefaultNotifier;

    private IConditionalSerializer GetSerializer(Type typeToSerialize)
    {
        // The Implementation assembly. If we do this in the constructor, the app doesn't start
        EnsureAssemblyInit(typeof(Serializer).Assembly);
        EnsureAssemblyInit(typeToSerialize.Assembly);
        
        if (_typeSerializerMap.TryGetValue(typeToSerialize, out var typeSerializer))
        {
            return typeSerializer;
        }

        foreach (var factory in _conditionalSerializerFactories)
        {
            if (factory.TryCreateSerializerFor(typeToSerialize) is { } serializer)
            {
                _typeSerializerMap.Add(typeToSerialize, serializer);
                return serializer;
            }
        }
        
        if (_conditionalSerializers.FirstOrDefault(serializer => serializer.SerializedTypeOrNull(typeToSerialize) is not null) is
            { } conditionalSerializer)
        {
            _typeSerializerMap.Add(typeToSerialize, conditionalSerializer);
            return conditionalSerializer;
        }

        if (PrimitiveSerializer.SerializedTypeOrNull(typeToSerialize) is not null)
        {
            _typeSerializerMap.Add(typeToSerialize, PrimitiveSerializer);
            return PrimitiveSerializer;
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
            if (type is { ContainsGenericParameters: false, IsAbstract: false, IsInterface: false }
                && type != typeof(PrimitiveSerializer)
                && type.GetInterfaces().Contains(typeof(IConditionalSerializer))
                && ActivatorUtilities.CreateInstance(serviceProvider, type) is IConditionalSerializer conditionalSerializer)
            {
                RegisterSerializer(conditionalSerializer);
            }

            if (type is { ContainsGenericParameters: false, IsAbstract: false, IsInterface: false } 
                && type.GetInterfaces().Contains(typeof(IConditionalSerializerFactory))
                && ActivatorUtilities.CreateInstance(serviceProvider, type) is IConditionalSerializerFactory factory)
            {
                RegisterFactory(factory);
            }
        }
    }

    private class NullNotifier : INotifySerializedValueChanged
    {
        public event EventHandler? SerializedValueChanged { add { } remove { } }

        public void Dispose()
        {
        }
    }
}