namespace Laminar.PluginFramework.Serialization;

public readonly struct DeserializationRequest<TValue, TSerialized>(DeserializationRequest untyped)
{
    public TSerialized Serialized { get; } = (TSerialized)untyped.Serialized;
    public object? Context { get; } = untyped.Context;
    public TValue? ExistingValue { get; } 
        = untyped.ExistingInstance switch
        {
            null => default,
            TValue typed => typed,
            _ => throw new InvalidCastException()
        };
    
    public bool HasExistingValue { get; } = untyped.ExistingInstance is not null;
}

public readonly struct DeserializationRequest
{
    public required object Serialized { get; init; }
    public required Type TargetType { get; init; }

    public object? ExistingInstance { get; init; }
    public object? Context { get; init; }
}