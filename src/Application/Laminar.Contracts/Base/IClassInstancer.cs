namespace Laminar.Contracts.Base;

public interface IClassInstancer
{
    public T CreateInstance<T>();

    public object? CreateInstance(Type type);
}
