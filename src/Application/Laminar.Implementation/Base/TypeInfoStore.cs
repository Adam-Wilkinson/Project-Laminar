using System;
using System.Collections.Generic;
using Laminar.Contracts.Base;
using Laminar.Domain;

namespace Laminar.Implementation.Base;

public class TypeInfoStore : ITypeInfoStore
{
    private static readonly TypeInfo BlankType = new("Unknown Type", null, null, "#FFFFFF", null);

    private readonly Dictionary<Type, TypeInfo> _typeInfoStore = [];

    public TypeInfo GetTypeInfoOrBlank(Type? type)
        => type is not null && _typeInfoStore.TryGetValue(type, out var typeInfo) ? typeInfo : BlankType;

    public bool RegisterType(Type type, TypeInfo typeInfo)
    {
        return _typeInfoStore.TryAdd(type, typeInfo);
    }

    public bool TryGetTypeInfo(Type type, out TypeInfo info) 
    {
        if (_typeInfoStore.TryGetValue(type, out var infoOutput))
        {
            info = infoOutput;
            return true;
        }

        info = BlankType;
        return false;
    }
}