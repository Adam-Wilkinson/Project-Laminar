using Laminar.Domain;

namespace Laminar.Contracts.Base;

public interface ITypeInfoStore
{
    public TypeInfo GetTypeInfoOrBlank(Type? type);

    public bool TryGetTypeInfo(Type type, out TypeInfo info);

    public bool RegisterType(Type type, TypeInfo typeInfo);
}
