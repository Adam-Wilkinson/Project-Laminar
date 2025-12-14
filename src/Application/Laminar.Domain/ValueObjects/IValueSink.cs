namespace Laminar.Domain.ValueObjects;

public interface IValueSink<in T>
{
    public T Value { set; }
}