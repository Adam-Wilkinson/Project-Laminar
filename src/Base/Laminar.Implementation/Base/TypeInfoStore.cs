using System;
using System.Collections.Generic;
using Laminar.Contracts.Base;
using Laminar.Domain;

namespace Laminar.Implementation;

public class TypeInfoStore : ITypeInfoStore
{
    readonly Dictionary<Type, TypeInfo> _typeInfoStore = new();

    public TypeInfo GetTypeInfoOrBlank(Type? type)
        => type is not null && _typeInfoStore.TryGetValue(type, out TypeInfo typeInfo)
            ? typeInfo
            : new TypeInfo("Unknown Type", null, null, "#FFFFFF", default);

    public bool RegisterType(Type type, TypeInfo typeInfo)
    {
        if (_typeInfoStore.ContainsKey(type))
        {
            return false;
        }

        _typeInfoStore.Add(type, typeInfo);
        return true;
    }

    public bool TryGetTypeInfo(Type type, out TypeInfo info) => _typeInfoStore.TryGetValue(type, out info);
}