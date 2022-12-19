using Laminar.Domain;

namespace Laminar.Contracts.UserInterface;

public interface IReadonlyTypeInfoStore
{
    public TypeInfo? GetTypeInfo(Type type);
}
