namespace Laminar.Contracts.Primitives;

public interface IClassInstancer
{
    public T CreateInstance<T>();

    public object? CreateInstance(Type type);
}
