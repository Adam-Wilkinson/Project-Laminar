using System;
using System.Collections.Generic;
using Laminar.Contracts.UserInterface;
using Laminar.Domain;
using Laminar.Domain.Exceptions;

namespace Laminar.Core;

public class TypeInfoStore : ITypeInfoStore
{
    readonly Dictionary<Type, TypeInfo> _typeInfoStore = new();

    public TypeInfo? GetTypeInfo(Type type)
    {
        if (!_typeInfoStore.TryGetValue(type, out TypeInfo typeInfo))
        {
            return new TypeInfo("Unknown Type", null, null, "#FFFFFF", default);
            // throw new TypeNotRegisteredException(type);
        }

        return typeInfo;
    }

    public bool RegisterType(Type type, TypeInfo typeInfo)
    {
        if (_typeInfoStore.ContainsKey(type))
        {
            return false;
        }

        _typeInfoStore.Add(type, typeInfo);
        return true;
    }
}