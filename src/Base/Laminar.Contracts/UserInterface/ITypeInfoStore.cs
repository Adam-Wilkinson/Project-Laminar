using Laminar.Domain;

namespace Laminar.Contracts.UserInterface;

public interface ITypeInfoStore : IReadonlyTypeInfoStore
{
    public bool RegisterType(Type type, TypeInfo typeInfo);
}
